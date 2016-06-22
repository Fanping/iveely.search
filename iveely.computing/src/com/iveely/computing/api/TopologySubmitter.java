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

import com.iveely.computing.component.InputExecutor;
import com.iveely.computing.component.LocalCluster;
import com.iveely.computing.component.OutputExecutor;
import com.iveely.computing.common.Message;
import com.iveely.computing.config.Paths;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.host.Luggage;
import com.iveely.computing.host.SlaveTopology;
import com.iveely.framework.net.AsynClient;
import com.iveely.framework.net.Packet;
import com.iveely.framework.text.Convertor;

import java.io.BufferedInputStream;
import java.io.ByteArrayOutputStream;
import java.io.FileInputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * Topology Submitter.
 */
public class TopologySubmitter {

    /**
     * All topology.
     */
    private static final TreeMap<String, Topology> topologies = new TreeMap<>();

    /**
     * Input executor.
     */
    private static final TreeMap<String, List<InputExecutor>> inputExecutors = new TreeMap<>();

    /**
     * Logger
     */
    private final static Logger logger = Logger.getLogger(TopologySubmitter.class.getName());

    /**
     * Submit topology to Iveely Computing.
     *
     * @param builder
     * @param args
     * @throws java.io.IOException
     */
    public static void submit(Topology builder, String[] args) throws IOException {

        // 0. Self-examination, the model is correct.
        //    Inspection standard is in cluster mode, the arguments are not null.
        if (builder.getExecuteType() == Topology.ExecuteType.CLUSTER && (args == null || args.length < 1)) {
            throw new IllegalArgumentException("When in cluster mode,please make sure arguments are not null,or maybe you should be in local mode?");
        }

        // 1.  If local mode to run topology.
        if (builder.getExecuteType() == Topology.ExecuteType.LOCAL) {
            if (args != null && args.length > 0) {
                throw new IllegalArgumentException("When in local mode,please do not give any arguments,or maybe you should be in cluster mode?");
            }
            LocalCluster local = new LocalCluster();
            local.submit(builder);
        }

        // 2. If master run to check.
        if (args != null && args.length == 2) {
            logger.info("Master start to running toplogy:" + builder.getName());
            if (topologies.containsKey(builder.getName())) {
                Coordinator.getInstance().deleteNode(Paths.getTopologyRoot(builder.getName()));
                logger.warn("Topology is already exist.");
            }

            // 2.1 Make sure config.
            String jarPath = args[0];
            String topology = args[1];
            int specifySlaveCount = builder.getSlaveCount();
            int workerCount = builder.getTotalWorkerCount();
            int total = workerCount;

            // 2.2 Get best slaves.
            logger.info("Master start to get best slaves:" + builder.getName());
            logger.info("1. Specify slave count:" + specifySlaveCount);
            logger.info("2. Specify worker count:" + workerCount);
            List<String> slaves = new ArrayList<>();
            for (int i = 0; i < specifySlaveCount && i < Luggage.performanceSlaves.size(); i++) {
                String slave = Luggage.performanceSlaves.get(i);
                int cnt = Luggage.slaves.get(slave);
                slaves.add(slave);
                workerCount += cnt;
            }

            // 2.3 worker distribution.
            int left = total % slaves.size();
            int avg = total / slaves.size();
            logger.info("3. Avg workers' count on each slave:" + avg);
            logger.info("4. left workers' count:" + left);
            List<Integer> distri = new ArrayList<>();
            slaves.stream().forEach((_item) -> {
                distri.add(avg);
            });
            for (int i = 0; i < left; i++) {
                distri.set(i, distri.get(i) + 1);
            }

            // 2.4 Configuration parameters.
            List<String> parameters = new ArrayList<>();
            List<String> allputs = builder.getAllputs();
            int lastIndex = 0;
            int hasDistr = 0;
            for (int cnt : distri) {
                StringBuilder par = new StringBuilder();
                for (int i = lastIndex; i < hasDistr + cnt; i++) {
                    par.append(allputs.get(i));
                    par.append(" ");
                }
                lastIndex += cnt;
                hasDistr += cnt;
                parameters.add(par.toString());
            }

            // 2.5 Send to slaves.
            byte[] content = null;
            try {
                ByteArrayOutputStream out;
                try (BufferedInputStream inputStream = new BufferedInputStream(new FileInputStream(jarPath))) {
                    out = new ByteArrayOutputStream(inputStream.available());
                    byte[] temp = new byte[inputStream.available()];
                    int size;
                    while ((size = inputStream.read(temp)) != -1) {
                        out.write(temp, 0, size);
                    }
                }
                content = out.toByteArray();
            } catch (IOException e) {
                logger.error("Topology submitter exception when convert jar to bytes.", e);
                throw e;
            }

            for (int i = 0; i < slaves.size(); i++) {
                try {
                    String[] infor = slaves.get(i).split(",");
                    String header = topology + ":" + parameters.get(i);
                    logger.info("5." + i + "(" + slaves.get(i) + "):" + parameters.get(i));
                    SlaveTopology.getInstance().set(slaves.get(i), builder.getName());

                    // 3. Build packet.
                    Packet packet = new Packet();
                    packet.setExecuteType(Message.ExecuteType.UPLOAD.ordinal());
                    packet.setMimeType(Message.MIMEType.APP.ordinal());
                    byte[] appName = Message.getBytes(header);
                    byte[] appNameSize = Convertor.int2byte(appName.length);
                    byte[] data = new byte[4 + appName.length + content.length];
                    System.arraycopy(appNameSize, 0, data, 0, 4);
                    System.arraycopy(appName, 0, data, 4, appName.length);
                    System.arraycopy(content, 0, data, 4 + appName.length, content.length);
                    packet.setData(data);
                    AsynClient client = new AsynClient(infor[0], Integer.parseInt(infor[1]), null);
                    client.send(packet);
                } catch (Exception e) {
                    logger.error("Toplogy submitter exception when send jar to slaves.", e);
                }
            }
            Coordinator.getInstance().setNodeValue(Paths.getTopologyFinished(builder.getName()), allputs.size() + "");
            Coordinator.getInstance().setNodeValue(Paths.getTopologySlaveCount(builder.getName()), builder.getSlaveCount() + "");
            topologies.put(builder.getName(), builder);

            return;
        }

        // 3. If slave run.
        if (args != null && args.length == 1) {
            List<String> puts = Arrays.asList(args[0].split(" "));
            HashMap<String, Object> userConfig = builder.getUserConfig();
            List<IInput> inputConfig = builder.getInputs();
            List<IOutput> outputConfig = builder.getOutputs();

            // 3. Try to run output.
            for (int i = outputConfig.size() - 1; i > -1; i--) {
                IOutput output = outputConfig.get(i);
                String name = output.getClass().getName();
                for (int j = 0; j < puts.size(); j++) {
                    if (puts.get(j).equals(name)) {
                        puts.set(j, "empty");
                        OutputExecutor executor = new OutputExecutor(builder.getName(), output, userConfig);
                        executor.initialize();
                        break;
                    }
                }
            }

            // 4. Try to run input.
            List<InputExecutor> exs = new ArrayList<>();
            for (int i = inputConfig.size() - 1; i > -1; i--) {
                IInput input = inputConfig.get(i);
                String name = input.getClass().getName();
                for (int j = 0; j < puts.size(); j++) {
                    if (puts.get(j).equals(name)) {
                        puts.set(j, "empty");
                        InputExecutor executor = new InputExecutor(builder.getName(), input, userConfig);
                        exs.add(executor);
                        Thread thread = new Thread(executor);
                        thread.start();
                        break;
                    }
                }
            }
            inputExecutors.put(builder.getName(), exs);
            topologies.put(builder.getName(), builder);
        }
    }

    /**
     * Stop a topology which in runing.
     *
     * @param tpName Name of topology.
     * @return Information.
     */
    public static String stop(String tpName) {
        if (inputExecutors.containsKey(tpName)) {
            List<InputExecutor> list = inputExecutors.get(tpName);
            list.stream().forEach((executor) -> {
                executor.stop();
            });
            return "In stopping.";
        } else {
            return "Not found topology:" + tpName;
        }
    }

    private TopologySubmitter() {
    }
}
