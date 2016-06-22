package com.iveely.database;

import java.util.Date;
import org.apache.log4j.Logger;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-4-1 19:41:37
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
