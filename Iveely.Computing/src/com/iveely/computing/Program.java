/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.computing;

import com.iveely.computing.config.Configurator;
import com.iveely.computing.host.Master;
import com.iveely.computing.node.Attribute;
import com.iveely.computing.node.Slave;
import com.iveely.computing.status.SystemConfig;
import com.iveely.computing.user.Console;
import java.io.IOException;
import org.apache.log4j.Logger;
import org.apache.zookeeper.KeeperException;

/**
 * Iveely computing entrance.
 *
 * @author liufanping@iveely.com
 */
public class Program {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Program.class.getName());

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        //testMaster();
        //testSlave();
        //testWebSocket();
        // testConsole();
        // testSubmit();
        //testJarExecutor();
        processArgs(args);
    }

    public static void processArgs(String[] args) {
        if (args.length > 2) {
            String ip = args[1].trim();
            Integer port = Integer.parseInt(args[2].trim());
            if (port > 0 && port < 65535) {

                // Update confige information
                if (null != args[0].toLowerCase()) {
                    switch (args[0].toLowerCase()) {
                        case "master":
                            // Cmd for master
                            try {
                                Master master = new Master(ip, port);
                                master.run();
                            } catch (IOException | KeeperException | InterruptedException e) {
                                logger.error(e);
                            }

                            logger.info("Master started on port:" + Configurator.getMasterPort());
                            break;
                        case "slave":
                            try {
                                // Cmd for slave
                                Attribute.getInstance().setFolder(SystemConfig.appFoler);
                                Slave slave = new Slave(ip, port);
                                slave.run();
                                logger.info("Slave started, master is " + SystemConfig.masterServer + ":" + SystemConfig.masterPort);
                            } catch (Exception e) {
                                logger.error(e);
                            }

                            break;
                        default:
                            // Cmd for console
                            try {
                                Console console = new Console(ip, port);
                                console.run();
                            } catch (Exception e) {
                                logger.error(e);
                            }

                            break;
                    }
                }

            } else {
                argsInvaid();
            }

        } else {
            argsError();
        }
    }

    /**
     * Arguments error.
     */
    private static void argsError() {
        logger.error("arguments error,example [master zkServer zkPort] or [slave zkServer zkPort]");
    }

    /**
     * Arguments invaid.
     */
    private static void argsInvaid() {
        logger.error("arguments invaid, 0<port<65536.");
    }
}
