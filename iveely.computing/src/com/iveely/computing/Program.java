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
package com.iveely.computing;

import com.iveely.computing.api.writable.StringWritable;
import com.iveely.computing.common.StreamPacket;
import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.config.Configurator;
import com.iveely.computing.host.Master;
import com.iveely.computing.node.Slave;
import com.iveely.computing.user.Console;
import com.iveely.computing.supervisor.Monitor;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Locale;
import org.apache.commons.lang.SerializationUtils;
import org.apache.log4j.Logger;
import org.apache.zookeeper.KeeperException;

/**
 * Iveely computing entrance.
 *
 * Parameters including "master","slave","console",and starting in this order.
 */
public class Program {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Program.class);

    /**
     * @param args the command line arguments
     * @throws java.io.IOException
     */
    public static void main(String[] args) throws IOException {
        if (args != null && args.length > 0) {
            logger.info("start computing with arguments:" + String.join(",", args));
            String type = args[0].toLowerCase(Locale.CHINESE);
            switch (type) {
                case "master":
                    launchMaster();
                    return;
                case "slave":
                    if (args.length == 4) {
                        ConfigWrapper.get().getSlave().setPort(Integer.parseInt(args[1]));
                        ConfigWrapper.get().getSlave().setSlot(Integer.parseInt(args[2]));
                        ConfigWrapper.get().getSlave().setSlotCount(Integer.parseInt(args[3]));
                    }
                    launchSlave();
                    return;
                case "supervisor":
                    launchSupervisor();
                    return;
                case "console":
                    launchConsole();
                    return;
            }
        }
        logger.error("arguments error,example [master | supervisor | slave | console]");
        System.out.println("press any keys to exit...");
        new BufferedReader(new InputStreamReader(System.in)).readLine();
    }

    /**
     * Launch master.
     */
    private static void launchMaster() {
        Configurator configurator = ConfigWrapper.get();
        try {
            Master master = new Master(configurator.getZookeeper().getAddress(), configurator.getZookeeper().getPort(), configurator.getMaster().getPassword());
            master.run();
            logger.info(String.format("master started on port: %d", ConfigWrapper.get().getMaster().getPort()));
        } catch (IOException | KeeperException | InterruptedException e) {
            logger.error("master setup exception.", e);
        }
    }

    /**
     * Launch slave.
     */
    private static void launchSlave() {
        Configurator configurator = ConfigWrapper.get();
        try {
            Slave slave = new Slave(configurator.getZookeeper().getAddress(), configurator.getZookeeper().getPort());
            slave.run();
            logger.info(String.format("Slave started on port: %d", ConfigWrapper.get().getSlave().getPort()));
        } catch (Exception e) {
            logger.error("slave setup exception", e);
        }
    }

    /**
     * Launch supervisor.
     */
    private static void launchSupervisor() {
        try {
            Monitor monitor = new Monitor();
            monitor.doAction();
            logger.info("monitor started.");
        } catch (Exception e) {
            logger.error("monitor setup exception", e);
        }
    }

    /**
     * Launch console.
     */
    private static void launchConsole() {
        try {
            Console console = new Console();
            console.run();
        } catch (Exception e) {
            logger.error("console setup exception", e);
        }
    }
}
