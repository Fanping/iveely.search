package com.iveely.database;

import com.iveely.framework.net.Server;
import com.iveely.database.ui.HostProvider;
import org.apache.log4j.Logger;

/**
 *
 * @author liufanping@iveely.com
 * @date 2014-12-26 20:27:32
 */
public class Engine {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Engine.class.getName());

    /**
     * Server for connect.
     */
    private Server server;

    /**
     * Store engine.
     */
    private static Engine engine;

    /**
     * Warehouse helper.
     */
    private static StoreHelper helper;

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

            // 3. Start up data service.
            server = new Server(helper, port);
            server.start();
            return true;
        } catch (Exception e) {
            logger.error(e);
        }
        return false;
    }
}
