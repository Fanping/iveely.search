package com.iveely.computing.status;

import com.iveely.computing.api.IInput;
import com.iveely.computing.api.IOutput;
import java.util.HashMap;
import java.util.List;

/**
 * System config information.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 21:09:54
 */
public class SystemConfig {

    /**
     * Zookeeper server.
     */
    public static String zkServer;

    /**
     * Zookeeper server port.
     */
    public static int zkPort;

    /**
     * Server address of master.
     */
    public static String masterServer;

    /**
     * Server port of master.
     */
    public static int masterPort = 8000;

    /**
     * All Inputs.
     */
    public final static HashMap<String, List<IInput>> inputs = new HashMap<>();

    /**
     * All Outputs.
     */
    public final static HashMap<String, List<IOutput>> outputs = new HashMap<>();

    /**
     * Is in debug.
     */
    public static boolean isDebug = true;

    /**
     * slot count for a slave.
     */
    public final static int slotCount = 6;

    /**
     * Slave information on zookeeper root path.
     */
    public final static String slaveRoot = "/iveely/slave";

    /**
     * Master information on zookeeper root path.
     */
    public final static String masterRoot = "/iveely/master";

    /**
     * App folder for store runing app.
     */
    public final static String appFoler = "app";

    /**
     * Current slave address.
     */
    public static String crSlaveAddress;

    /**
     * Current slave port.
     */
    public final static int crSlavePort = 8001;

    /**
     * Base port of the slot.
     */
    public final static int slotBasePort = 6000;

    /**
     * Max worker count for a slave.
     */
    public final static int maxWorkerCount = 50;

    /**
     * Port for UI.
     */
    public final static int uiPort = 9000;

    /**
     * Current Iveely computing version.
     */
    public final static String version = "0.8.0";
}
