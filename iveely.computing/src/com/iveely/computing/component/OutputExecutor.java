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
package com.iveely.computing.component;

import com.iveely.computing.api.IOutput;
import com.iveely.computing.api.StreamChannel;
import com.iveely.computing.api.DataTuple;
import com.iveely.computing.config.Paths;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.node.Communicator;
import java.util.Date;
import java.util.HashMap;
import org.apache.log4j.Logger;

/**
 * Output Executor.
 */
public class OutputExecutor extends IExecutor {

    /**
     * Logger
     */
    private final Logger logger = Logger.getLogger(OutputExecutor.class.getName());

    /**
     * Input worker.
     */
    private final IOutput _output;

    /**
     * Execute count for statistic.
     */
    private int _executeCount;

    /**
     * Start time.
     */
    private final long _startTime;

    /**
     * Slot.
     */
    private final Communicator.Slot _slot;

    /**
     * Build output executor.
     *
     * @param name Name of the output.
     * @param output The output instance.
     * @param conf User-defined configuration information.
     */
    public OutputExecutor(String name, IOutput output, HashMap<String, Object> conf) {
        this._output = output;
        this._conf = conf;
        this._executeCount = 0;
        this._name = name;
        this._slot = Communicator.getInstance().selectSlot();
        this._collector = new StreamChannel(output.getName(), name, this);
        this._slot.register(_output.getName(), this._collector);
        this._startTime = new Date().getTime();
        String connectPath = this._slot.getConnectString();
        Coordinator.getInstance().setNodeValue(Paths.getTopologyOutputRecord(this._name, output.getClass().getName(), output.getName()), connectPath);
    }

    /**
     * For the Output initialization.
     */
    public void initialize() {
        this._output.start(this._conf);
        this._output.toOutput(this._collector);
    }

    /**
     * Invoke tuple when data arrive.
     *
     * @param tuple
     */
    public void invoke(DataTuple tuple) {
        synchronized (this._output) {
            try {
                this._output.execute(tuple, this._collector);
                this._executeCount++;
                if (this._executeCount % 100 == 0) {
                    double hour = (new Date().getTime() - this._startTime) / (1000.0 * 60 * 60);
                    Coordinator.getInstance().setNodeValue(
                            Paths.getTopologyOutputExecute(this._name, this._output.getName()),
                            this._executeCount + "");
                    Coordinator.getInstance().setNodeValue(
                            Paths.getTopologyOutputAvgExecute(this._name, this._output.getName()),
                            this._executeCount / hour + "(execute)/h");
                }
                //System.out.println("INVOKE:" + this._executeCount);
            } catch (Exception e) {
                logger.error("Output executor exception.", e);
                StringBuilder error = new StringBuilder();
                error.append(e.getMessage());
                error.append("<br/>");
                StackTraceElement[] trace = e.getStackTrace();
                for (StackTraceElement s : trace) {
                    error.append("   ");
                    error.append(s);
                    error.append("<br/>");
                }
                Coordinator.getInstance().setNodeValue(Paths.getTopologyStatusRoot(this._name) + "/output/" + this._output.getName()
                        + "/" + this._slot.getConnectString(), error.toString());
                Coordinator.getInstance().setNodeValue(Paths.getTopologyFinished(this._name), -1 + "");
                Coordinator.getInstance().setNodeValue(Paths.getTopologyStopped(this._name, this._output.getName()),
                        "Exception stopped.");
            }
        }
    }

    /**
     * All data has finish send.
     */
    public void end() {
        this._slot.unregister(this._output.getName());
        this._output.end(this._conf);
        Coordinator.getInstance().setNodeValue(
                Paths.getTopologyStatusRoot(this._name) + "/output/" + this._output.getName() + "/execute",
                this._executeCount + "");
        Coordinator.getInstance().setNodeValue(Paths.getTopologyStopped(this._name, this._output.getName()),
                "End.");
    }
}
