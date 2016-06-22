package com.iveely.database.net;

import com.iveely.framework.net.SyncServer;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-11 8:24:22
 */
public class ConnectPool {

	/**
	 * Connect server.
	 */
	private final SyncServer server;

	/**
	 * Event of connection.
	 */
	private final ConnectEvent event;

	public ConnectPool(int port) {
		this.event = new ConnectEvent();
		this.server = new SyncServer(this.event, port);
	}

	public void start() {
		this.server.start();
	}
}
