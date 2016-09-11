package com.iveely.database.ui;

import com.iveely.framework.net.websocket.SocketServer;

import org.apache.log4j.Logger;

/**
 * @author liufanping@iveely.com
 */
public class HostProvider implements Runnable {

  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(HostProvider.class.getName());
  /**
   * Websocket server.
   */
  private SocketServer socket;
  /**
   * Response callback.
   */
  private Response response;

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
