/*
 * Copyright 2016 liufanping@iveely.com.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.iveely.computing.api;

import com.iveely.computing.api.writable.IWritable;
import com.iveely.computing.common.IStreamCallback;
import com.iveely.computing.component.OutputExecutor;
import com.iveely.computing.component.IExecutor;
import com.iveely.computing.common.StreamPacket;
import com.iveely.computing.common.StreamType;
import com.iveely.computing.component.TupleBuffer;
import com.iveely.computing.config.Paths;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.framework.net.AsynClient;
import com.iveely.framework.net.Packet;
import com.iveely.framework.process.ThreadUtil;
import com.iveely.framework.text.Convertor;

import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import org.apache.commons.lang.SerializationUtils;
import org.apache.log4j.Logger;

/**
 * Stream Channel,Data flow transmission carrier. Each Input holds an
 * independent of the stream channel.
 *
 */
public class StreamChannel implements IStreamCallback {

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(StreamChannel.class);

    /**
     * Data sent after received confirmation. Guarantee reliable data
     * transmission.The amount of calculation method for sending data must be
     * equal to the amount of data.
     */
    private class AckHandler implements AsynClient.IHandler {

        @Override
        public void receive(Packet packet) {
            synchronized (this) {
                if (packet.getExecuteType() == StreamType.SUCCESS.ordinal()) {
                    ackedCount += Convertor.bytesToInt(packet.getData());
                }
            }
        }

        @Override
        public void caught(String exception) {
            logger.error(String.format("Ack exception in stream channel,message is %s.", exception));
        }
    }

    /**
     * Calibration value,Send the size of the data size must be equal to the
     * received data.
     */
    private long sendCount;

    /**
     * Acked count from others.
     */
    private long ackedCount;

    /**
     * Has finish end of output data.
     */
    private boolean isTransmitted;

    /**
     * Output executor.
     */
    private final IExecutor executor;

    /**
     * For next output clients.
     */
    private final TreeMap<String, List<AsynClient>> outputClients;

    /**
     * For next output output.
     */
    private final TreeMap<String, List<String>> outputGuids;

    /**
     * Emit count.
     */
    private int emitCount;

    /**
     * Time of start.
     */
    private final long start;

    /**
     * Name of the input.
     */
    private final String inputName;

    /**
     * Name of the topology.
     */
    private final String name;

    /**
     * Cache buffer to send tuple.
     */
    private final TupleBuffer buffer;

    /**
     * Build stream channel.
     *
     * @param inputName
     * @param name
     * @param executor
     */
    public StreamChannel(String inputName, String name, IExecutor executor) {
        this.isTransmitted = false;
        this.executor = executor;
        this.outputClients = new TreeMap<>();
        this.outputGuids = new TreeMap<>();
        this.name = name;
        this.emitCount = 0;
        this.inputName = inputName;
        this.start = new Date().getTime();
        this.buffer = new TupleBuffer();
        this.ackedCount = 0;
        this.sendCount = 0;
    }

    /**
     * Emit data to output.
     *
     * @param key
     * @param value
     */
    public void emit(IWritable key, IWritable value) {
        // 1. record emit count on zookeeper.
        recordEmitCount(emitCount++);

        this.outputClients.entrySet().stream().forEach((entry) -> {
            List<AsynClient> list = (List<AsynClient>) entry.getValue();
            int code = Math.abs(key.hashCode() % list.size());

            // 2. Prepare stream packet.
            StreamPacket streamPacket = new StreamPacket();
            streamPacket.setData(SerializationUtils.serialize(new DataTuple(key, value)));
            streamPacket.setType(StreamType.DATASENDING);
            streamPacket.setGuid((String) this.outputGuids.get(entry.getKey()).get(code));
            streamPacket.setName(this.name);
            buffer.push(code, streamPacket);

            // 3. Check buffer, if reach state of full will be send.
            checkBuffer(code, list.get(code));
        });
    }

    /**
     * Told IOutput, IInput emit end.
     *
     * @throws java.lang.Exception
     */
    public void emitEnd() throws Exception {
        isTransmitted = true;
        int maxSeconds = 10;
        Coordinator.getInstance().setNodeValue(Paths.getInputTotalEmit(this.name, this.inputName),
                emitCount + "");
        for (Map.Entry entry : this.outputClients.entrySet()) {
            String key = entry.getKey().toString();
            List<AsynClient> list = (List<AsynClient>) entry.getValue();
            for (int i = 0; i < list.size(); i++) {
                StreamPacket streamPacket = new StreamPacket();
                streamPacket.setType(StreamType.END);
                streamPacket.setGuid(this.outputGuids.get(key).get(i));
                streamPacket.setName(this.name);
                buffer.push(i, streamPacket);
                Packet packet = new Packet();
                packet.setData(buffer.pop(i));
                while (sendCount - ackedCount > 0 && maxSeconds > 0) {
                    ThreadUtil.sleep(1);
                    maxSeconds--;
                }
                if (maxSeconds < 1 && sendCount - ackedCount > 0) {
                    throw new Exception("Cannot emit end as data validation error.");
                } else {
                    list.get(i).send(packet);
                }
            }
        }

        Coordinator.getInstance().setNodeValue(Paths.getInputEmitFinished(this.name, this.inputName), "Emit end.");
    }

    /**
     * Whether the data stream is transmitted.
     *
     * @return True is transmitted.
     */
    public boolean isTransmitted() {
        return isTransmitted;
    }

    /**
     * New Output for stream channel.
     *
     * @param ciout The class of new output.
     * @throws java.lang.InstantiationException
     * @throws java.lang.IllegalAccessException
     */
    public void addOutputTo(Class<? extends IOutput> ciout) throws InstantiationException, IllegalAccessException {
        IOutput output = ciout.newInstance();
        String rootPath = Paths.getRecordExecutorOutput(executor.getName(), output.getClass().getName());
        int retryCount = 5;
        List<String> children;
        List<AsynClient> clients = new ArrayList<>();
        List<String> guids = new ArrayList<>();
        do {
            children = Coordinator.getInstance().getChildren(rootPath);
            for (String cd : children) {
                String path = rootPath + "/" + cd;
                String guid = cd;
                String value = Coordinator.getInstance().getNodeValue(path);
                if (!value.isEmpty()) {
                    String[] infor = value.split(",");
                    AsynClient client = new AsynClient(infor[0], Integer.parseInt(infor[1]), new AckHandler());
                    clients.add(client);
                    guids.add(guid);
                }
            }
            if (children.isEmpty()) {
                retryCount--;
                ThreadUtil.sleep(2);
            }
        } while (children.isEmpty() && retryCount > -1);
        String key = output.getClass().getName();
        if (!this.outputClients.containsKey(key)) {
            this.outputClients.put(key, clients);
            this.outputGuids.put(key, guids);
        }
    }

    @Override
    public void invoke(StreamPacket packet) {
        OutputExecutor outputExecutor;
        if (executor instanceof OutputExecutor) {
            outputExecutor = (OutputExecutor) executor;
        } else {
            return;
        }

        int streamType = packet.getType().ordinal();
        if (streamType == StreamType.DATASENDING.ordinal()) {
            byte[] data = packet.getData();
            DataTuple tuple = (DataTuple) SerializationUtils.deserialize(data);
            outputExecutor.invoke(tuple);
        } else if (streamType == StreamType.END.ordinal()) {
            outputExecutor.end();
        }
    }

    /**
     * Record emit count for current IInput.
     *
     * @param emitCount Emit count.
     */
    private void recordEmitCount(Integer emitCount) {
        if (emitCount % 100 == 0) {
            long diff = new Date().getTime() - start;
            double hour = diff * 1.0 / (1000 * 60 * 60);
            Coordinator.getInstance()
                    .setNodeValue(Paths.getInputTotalEmit(this.name, this.inputName), emitCount + "");
            Coordinator.getInstance().setNodeValue(
                    Paths.getInputAvgEmit(this.name, this.inputName),
                    emitCount / hour + "(emit)/h");
        }
    }

    /**
     * Check buffer to send.
     *
     * @param code Buffer code.
     * @param client The client to send buffer.
     */
    private void checkBuffer(Integer code, AsynClient client) {
        if (buffer.isFull(code)) {
            byte[] tuples = buffer.pop(code);
            if (tuples.length > 0) {
                Packet packet = new Packet();
                packet.setData(tuples);
                if (client.send(packet)) {
                    this.sendCount += buffer.getMaxStored();
                } else {
                    logger.error("Send failure.");
                }
            }
        }
    }
}
