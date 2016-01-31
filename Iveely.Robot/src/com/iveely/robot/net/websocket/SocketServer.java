package com.iveely.robot.net.websocket;

import java.io.IOException;
import java.net.ServerSocket;
import org.apache.log4j.Logger;
import org.eclipse.jetty.server.Server;

/**
 * Websocket.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-2 22:26:28
 */
public class SocketServer {

	public interface IHandler {

		/**
		 * Call back.
		 *
		 * @param data
		 * @return
		 */
		public String invoke(Integer sessionId, WSHandler handler, String data);

		/**
		 * Call back of close event.
		 * 
		 * @param sessionId
		 */
		public void close(Integer sessionId);
	}

	/**
	 * Service port.
	 */
	private int port = 8000;

	/**
	 * Service socket.
	 */
	private ServerSocket serverSocket;

	/**
	 * Event processor.
	 */
	private IHandler eventProcessor;

	/**
	 * logger.
	 */
	private final Logger logger = Logger.getLogger(SocketServer.class.getName());

	public SocketServer(IHandler processor, int port) throws IOException {
		if (processor == null) {
			throw new NullPointerException();
		}
		this.port = port;
		this.eventProcessor = processor;
		logger.info("Web socket service is init.");
	}

	public void start() {
		try {
			Server server = new Server(this.port);
			WSHandler.setProcessor(eventProcessor);
			server.setHandler(new WSHandler());
			server.setStopTimeout(0);
			server.start();
			server.join();
		} catch (Exception ex) {
			logger.error(ex);
		}
	}
}
