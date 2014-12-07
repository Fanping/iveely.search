package com.iveely.computing.node;

import com.iveely.framework.net.Server;
import org.apache.log4j.Logger;

/**
 * Slave machine.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:05:29
 */
public class Slave implements Runnable {

    /**
     * Service port.
     */
    private int port;

    /**
     * Event server.
     */
    private final Server server;

    /**
     * Heartbeat, send each minitue.
     */
    private final Heartbeat heartbeat;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Slave.class.getName());

    /**
     * Event processor.
     */
    private final SlaveProcessor processor;

    public Slave(Integer port) {
        heartbeat = new Heartbeat(port);
        processor = new SlaveProcessor();
        server = new Server(processor, port);
    }

    @Override
    public void run() {

        // 1.Startup heartbeat.
        Thread heartThread = new Thread(heartbeat);
        heartThread.start();
        logger.info("heartbeat is started.");

        // 2.Init segment.
        logger.info("load segement..");
        com.iveely.framework.segment.Markov.getInstance();

        // 3.Startup schedule service.
        Thread scheduleThread = new Thread(Schedule.getInstance());
        scheduleThread.start();
        logger.info("schedule is started.");

        // 4.Startup event listen.
        server.start();
    }
}
