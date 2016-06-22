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
package com.iveely.computing.node;

import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.framework.net.AsynServer;

import java.io.IOException;
import java.util.Date;
import org.apache.log4j.Logger;
import org.apache.zookeeper.KeeperException;

/**
 * Slave machine.
 *
 * @author Iveely Liu
 */
public class Slave implements Runnable {

    /**
     * Event server.
     */
    private final AsynServer server;

    /**
     * Heartbeat, send each minitue.
     */
    private final Heartbeat heartbeat;

    private Thread heartThread;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Slave.class.getName());

    /**
     * Event processor.
     */
    private final SlaveProcessor processor;

    /**
     * Build slave.
     *
     * @param zkServer Zookeeper server address.
     * @param zkPort Zookeeper server port.
     * @throws IOException
     * @throws KeeperException
     * @throws InterruptedException
     * @throws Exception
     */
    public Slave(String zkServer, int zkPort) throws IOException, KeeperException, InterruptedException, Exception {
        this.processor = new SlaveProcessor();
        this.server = new AsynServer(ConfigWrapper.get().getSlave().getPort(), processor);
        initZookeeper(zkServer, zkPort, ConfigWrapper.get().getSlave().getPort());
        this.heartbeat = new Heartbeat();
        Communicator.getInstance();
    }

    /**
     * Start to run slave.
     */
    @Override
    public void run() {
        // 1.Startup heartbeat.
        Thread.currentThread().setName("slave main thread");
        heartThread = new Thread(heartbeat);
        heartThread.start();
        logger.info("Heartbeat thread is started.");

        // 2.Startup event listen.
        try {
            logger.info("Startup event listen.");
            server.open();
        } catch (IOException e) {
            logger.error("When slave setup event listenner,exception happend.", e);
        }
    }

    /**
     * Init information to zookeeper.
     *
     * @param server
     * @param port
     * @param slavePort
     * @throws IOException
     * @throws KeeperException
     * @throws InterruptedException
     */
    private void initZookeeper(String server, int port, int slavePort)
            throws IOException, KeeperException, InterruptedException {

        // 1. Create zookeeper.
        // 2. Create root\master
        String slave = com.iveely.framework.net.Internet.getLocalIpAddress()
                + "," + slavePort;
        Coordinator.getInstance().setNodeValue(ConfigWrapper.get().getSlave().getRoot() + "/" + slave, new Date().toString());
        logger.info("slave information:" + slave);
    }

    /**
     * Stop slave.
     */
    public void stop() {
        this.heartbeat.stop();
        Communicator.getInstance().close();
        if (server != null) {
            server.close();
        }
    }
}
