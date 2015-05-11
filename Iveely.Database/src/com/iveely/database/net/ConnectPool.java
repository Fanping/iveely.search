package com.iveely.database.net;

import com.iveely.framework.net.Server;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-11 8:24:22
 */
public class ConnectPool {

    /**
     * Connect server.
     */
    private final Server server;

    /**
     * Event of connection.
     */
    private final ConnectEvent event;

    public ConnectPool(int port) {
        this.event = new ConnectEvent();
        this.server = new Server(this.event, port);
    }

    public void start() {
        this.server.start();
    }
}
