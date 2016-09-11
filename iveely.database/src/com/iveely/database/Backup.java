package com.iveely.database;

import org.apache.log4j.Logger;

import java.util.Date;

/**
 * @author liufanping@iveely.com
 */
public class Backup implements Runnable {

  /**
   * Logger.
   */
  private static final Logger logger = Logger.getLogger(Backup.class.getName());

  public void run() {
    while (true) {
      try {
        try {
          String[] names = LocalStore.getDatabases();
          for (String name : names) {
            LocalStore.backupWarehouse(name, name + "_bak_" + new Date().getTime());
          }
        } catch (Exception e) {
          logger.error(e);
        }
        Thread.sleep(1000 * 60 * 60 * 12);
      } catch (InterruptedException ex) {
        logger.error(ex);
      }
    }
  }

}
