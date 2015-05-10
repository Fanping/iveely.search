package com.iveely.computing.config;

import com.iveely.database.LocalStore;
import com.iveely.database.storage.Types;
import com.iveely.database.storage.Warehouse;

/**
 * Configurator.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:21:57
 */
public class Configurator {

    /**
     * Store warehouse.
     */
    private static Warehouse warehouse;

    /**
     * Master's deploy.
     */
    private static MasterDeploy master;

    /**
     * Init.
     */
    private static void Init() {
        if (warehouse == null) {
            warehouse = LocalStore.getWarehouse("Config");
            warehouse.createTable("MasterDeploy");
            warehouse.createColumn("MasterDeploy", "port", Types.INTEGER, false);
            warehouse.createColumn("MasterDeploy", "server", Types.STRING, false);
        }
    }

    /**
     * Load config information.
     */
    private static void load() {

        // 1. Init warehouse.
        Init();

        // 2. Load master.
        if (master == null) {
            master = MasterDeploy.GetDefault();
            Object[] objs = warehouse.selectById("MasterDeploy", 0);
            if (objs != null) {
                master.setPort((int) objs[0]);
                master.setHostAddress((String) objs[1]);
            } else {
                warehouse.insert("Config", new Object[]{
                    master.getPort(),
                    master.getHostAddress()
                });
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
        warehouse.update("MasterDeploy", new Object[]{master.getPort(), master.getHostAddress()}, 0);
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
        warehouse.update("MasterDeploy", new Object[]{master.getPort(), master.getHostAddress()}, 0);
    }
}
