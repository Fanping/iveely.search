package com.iveely.database.ui;

import org.apache.log4j.Logger;

import com.iveely.framework.net.websocket.SocketServer;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-7 23:04:49
 */
public class HostProvider implements Runnable {

	/**
	 * Websocket server.
	 */
	private SocketServer socket;

	/**
	 * Response callback.
	 */
	private Response response;

	/**
	 * Logger.
	 */
	private final Logger logger = Logger.getLogger(HostProvider.class.getName());

	public HostProvider() {
		try {
			this.response = new Response();
			this.socket = new SocketServer(this.response, 4322);
		} catch (Exception e) {
			logger.error(e);
		}

	}

	@Override
	public void run() {
		logger.info("UI service is starting...");
		this.socket.start();
	}
}
