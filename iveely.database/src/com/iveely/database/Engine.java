package com.iveely.database;

import com.iveely.database.ui.HostProvider;
import com.iveely.framework.net.SyncServer;

import org.apache.log4j.Logger;

/**
 * @author liufanping@iveely.com
 * @date 2014-12-26 20:27:32
 */
public class Engine {

  /**
   * Logger.
   */
  private static final Logger logger = Logger.getLogger(Engine.class.getName());
  /**
   * Store engine.
   */
  private static Engine engine;
  /**
   * Warehouse helper.
   */
  private static StoreHelper helper;
  /**
   * Server for connect.
   */
  private SyncServer server;

  private Engine() {
    helper = new StoreHelper();
  }

  public static Engine getInstance() {
    if (engine == null) {
      engine = new Engine();
    }
    return engine;
  }

  public boolean start(int port) {
    try {

      // 1. Start up ui service.
      HostProvider provider = new HostProvider();
      Thread ui = new Thread(provider);
      ui.start();

      // 2. Backup
      Backup backup = new Backup();
      Thread backThread = new Thread(backup);
      backThread.start();

      // 3. Flusher
      FlushTimer timer = new FlushTimer();
      Thread flThread = new Thread(timer);
      flThread.start();

      // 4. Start up data service.
      server = new SyncServer(helper, port);
      server.start();
      return true;
    } catch (Exception e) {
      logger.error(e);
    }
    return false;
  }
}
