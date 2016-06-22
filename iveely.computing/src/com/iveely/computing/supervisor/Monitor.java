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
package com.iveely.computing.supervisor;

import com.iveely.computing.config.ConfigWrapper;
import com.iveely.framework.process.ThreadUtil;
import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;

/**
 * Monitor for node. Used to start and manage slave node.
 */
public class Monitor {

    /**
     * Logger.
     */
    private static Logger logger = Logger.getLogger(Monitor.class);

    /**
     * The slave service port.
     */
    private int slavePort;

    /**
     * Slot on slave basic port to provide service.
     */
    private int slotBasicPort;

    /**
     * Allow slot count on a slave.
     */
    private int slotCount;

    /**
     * All slaves on current machine.
     */
    private List<Process> slaves;

    /**
     * The jar path of slave.
     */
    private String path;

    /**
     * Max slaves count allow on current machine.
     */
    private int maxSlaves;

    /**
     * Build monitor instance.
     */
    public Monitor() {
        this.slavePort = ConfigWrapper.get().getSlave().getPort();
        this.slotBasicPort = ConfigWrapper.get().getSlave().getSlot();
        this.slotCount = ConfigWrapper.get().getSlave().getSlotCount();
        this.slaves = new ArrayList<>();
        this.path = Monitor.class.getProtectionDomain().getCodeSource().getLocation().getPath();
        if (this.path.contains(":")) {
            this.path = this.path.split(":")[1];
        }
        this.maxSlaves = 2;
    }

    /**
     * Do action for monitor.
     */
    public void doAction() {
        logger.info(String.format("jar path is %s .", this.path));
        while (true) {
            for (int i = 0; i < this.slaves.size(); i++) {
                if (!this.slaves.get(i).isAlive()) {
                    logger.info("remove useless process");
                    this.slaves.remove(i);
                }
            }
            int left = maxSlaves - this.slaves.size();
            for (int i = 0; i < left; i++) {
                logger.info("start process.");
                startProcess();
            }
            ThreadUtil.sleep(60);
        }
    }

    /**
     * Start a slave process.
     */
    private void startProcess() {
        String args = "slave " + this.slavePort + " " + this.slotBasicPort + " " + this.slotCount;
        logger.info(String.format("start process,jar path:%s,arguments:%s", path, args));
        Process process = com.iveely.computing.common.ProcessBuilder.start(path, args);
        if (process != null && process.isAlive()) {
            this.slavePort++;
            this.slotBasicPort += this.slotCount;
            this.slaves.add(process);
            logger.info("process is started.");
        } else {
            logger.info("process start failure.");
        }
    }
}
