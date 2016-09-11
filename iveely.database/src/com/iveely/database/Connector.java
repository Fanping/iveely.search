package com.iveely.database;

import com.iveely.database.message.CloseStatus;
import com.iveely.database.message.OpenStatus;

/**
 * Connector for database.
 *
 * @author liufanping@iveely.com
 */
public class Connector {

  /**
   * The server to connect.
   */
  private String server;

  /**
   * @return the server
   */
  public String getServer() {
    return server;
  }

  /**
   * Open connection.
   */
  public OpenStatus open(String server, String port, String dbName) {
    this.server = server;
    return OpenStatus.NOT_FOUND_HOST;
  }

  /**
   * Close connection.
   */
  public CloseStatus close() {
    return CloseStatus.SUCCESS;
  }
}
