package com.iveely.computing.config;

import com.iveely.framework.database.Engine;

/**
 * Configurator.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:21:57
 */
public class Configurator {

    /**
     * Store engine.
     */
    private static Engine engine;

    /**
     * Master's deploy.
     */
    private static MasterDeploy master;

    /**
     * Init.
     */
    private static void Init() {
        if (engine == null) {
            engine = new Engine();
            engine.createDatabase("Config");
            engine.createTable(new MasterDeploy());
        }
    }

    /**
     * Load config information.
     */
    private static void load() {

        // 1. Init engine.
        Init();

        // 2. Load master.
        if (master == null) {
            master = (MasterDeploy) engine.read(new MasterDeploy(), 0);
            if (master == null) {
                master = MasterDeploy.GetDefault();
                engine.write(master);
            }
        }
    }

    /**
     * Get master's ip address.
     *
     * @return
     */
    public static String getMasterAddress() {

        // 1. Load all information.
        load();

        // 2. Get and return.
        return master.getHostAddress();
    }

    /**
     * Get master's port.
     *
     * @return
     */
    public static int getMasterPort() {

        // 1. Load all information.
        load();

        // 2. Get and return.
        return master.getPort();
    }

    /**
     * Update master's address.
     *
     * @param address
     */
    public static void updateMasterAddress(String address) {

        // 1. Load all information.
        load();

        // 2. Set in memory.
        master.setHostAddress(address);

        // 3. Disk update.
        engine.update(0, master);
    }

    /**
     * Update master's port.
     *
     * @param port
     */
    public static void updateMasterPort(int port) {

        // 1. Load all inforamtion.
        load();

        // 2. Set in memory.
        master.setPort(port);

        // 3. Disk update.
        engine.update(0, master);
    }
}
