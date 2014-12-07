package com.iveely.computing.host;

import com.iveely.computing.config.Configurator;
import com.iveely.framework.net.Server;
import org.apache.log4j.Logger;

/**
 * Master.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:06:47
 */
public class Master implements Runnable {

    /**
     * Event server from slave to master.
     */
    private final Server server;

    /**
     * Event processor from slave to master.
     */
    private final MasterProcessor slaveProcessor;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Master.class.getName());

    public Master() {
        slaveProcessor = new MasterProcessor();
        server = new Server(slaveProcessor, Configurator.getMasterPort());
    }

    /**
     * Start master.
     */
    @Override
    public void run() {
        try {
            server.start();
        } catch (Exception ex) {
            logger.error(ex);
        }

    }
}
