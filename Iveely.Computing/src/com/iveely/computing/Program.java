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
import com.iveely.computing.user.Console;
import com.iveely.framework.net.cache.Memory;
import org.apache.log4j.Logger;

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
        if (args.length > 2) {
            String ip = args[1].trim();
            Integer port = Integer.parseInt(args[2].trim());
            if (port > 0 && port < 65535) {

                // Update confige information
                Configurator.updateMasterAddress(ip);
                Configurator.updateMasterPort(port);

                if (null != args[0].toLowerCase()) {
                    switch (args[0].toLowerCase()) {
                        case "master":
                            // Cmd for master
                            Master master = new Master();
                            master.run();
                            logger.info("Master started on port:" + Configurator.getMasterPort());
                            break;
                        case "slave":
                            Integer slavePort = Integer.parseInt(args[3]);
                            // Cmd for slave
                            Attribute.getInstance().setFolder(slavePort.toString());
                            Attribute.getInstance().loadApps();
                            Slave slave = new Slave(slavePort);
                            slave.run();
                            logger.info("Slave started, master is " + Configurator.getMasterAddress() + ":" + Configurator.getMasterPort());
                            break;
                        default:
                            // Cmd for console
                            Console console = new Console();
                            console.run();
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
        logger.error("arguments error,example [master IP Port] or [slave slavePort masterIP masterPort]");
    }

    /**
     * Arguments invaid.
     */
    private static void argsInvaid() {
        logger.error("arguments invaid, 0<port<65536.");
    }
}
