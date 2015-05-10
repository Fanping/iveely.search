package com.iveely.computing.ui;

import com.iveely.computing.status.SystemConfig;
import com.iveely.framework.text.json.JsonUtil;

/**
 * System summary.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-8 15:15:15
 */
public final class SystemSummary {

    /**
     * Response type.
     */
    private String resType;

    /**
     * Zookeeper server.
     */
    private String zkServer;

    /**
     * Zookeeper server port.
     */
    private int zkPort;

    /**
     * Server address of master.
     */
    private String masterServer;

    /**
     * Server port of master.
     */
    private int masterPort;

    /**
     * slot count for a slave.
     */
    private int slotCount;

    /**
     * Slave information on zookeeper root path.
     */
    private String slaveRoot;

    /**
     * Master information on zookeeper root path.
     */
    private String masterRoot = "/iveely/master";

    /**
     * App folder for store runing app.
     */
    private String appFoler = "app";

    /**
     * Current slave port.
     */
    private int crSlavePort = 8001;

    /**
     * Base port of the slot.
     */
    private int slotBasePort = 6000;

    /**
     * Max worker count for a slave.
     */
    private int maxWorkerCount = 50;

    /**
     * Port for UI.
     */
    private int uiPort = 9000;

    public void init() {
        this.setAppFoler();
        this.setCrSlavePort();
        this.setMasterPort();
        this.setMasterRoot();
        this.setMasterServer();
        this.setMaxWorkerCount();
        this.setSlaveRoot();
        this.setSlotBasePort();
        this.setSlotCount();
        this.setUiPort();
        this.setZkPort();
        this.setZkServer();
        this.resType = "system summary";
    }

    /**
     * Get response type.
     *
     * @return
     */
    public String getResType() {
        return this.resType;
    }

    /**
     * @return the zkServer
     */
    public String getZkServer() {
        return zkServer;
    }

    /**
     * Set zookeeper server.
     */
    public void setZkServer() {
        this.zkServer = SystemConfig.zkServer;
    }

    /**
     * @return the zkPort
     */
    public int getZkPort() {
        return zkPort;
    }

    /**
     * Set zookeeper port.
     */
    public void setZkPort() {
        this.zkPort = SystemConfig.zkPort;
    }

    /**
     * @return the masterServer
     */
    public String getMasterServer() {
        return masterServer;
    }

    /**
     * Set master server.
     */
    public void setMasterServer() {
        this.masterServer = SystemConfig.masterServer;
    }

    /**
     * @return the masterPort
     */
    public int getMasterPort() {
        return masterPort;
    }

    /**
     * Set master port.
     */
    public void setMasterPort() {
        this.masterPort = SystemConfig.masterPort;
    }

    /**
     * @return the slotCount
     */
    public int getSlotCount() {
        return slotCount;
    }

    /**
     * Set slot count.
     */
    public void setSlotCount() {
        this.slotCount = SystemConfig.slotCount;
    }

    /**
     * @return the slaveRoot
     */
    public String getSlaveRoot() {
        return slaveRoot;
    }

    /**
     * Set slave root path.
     */
    public void setSlaveRoot() {
        this.slaveRoot = SystemConfig.slaveRoot;
    }

    /**
     * @return the masterRoot
     */
    public String getMasterRoot() {
        return masterRoot;
    }

    /**
     * Set master root.
     */
    public void setMasterRoot() {
        this.masterRoot = SystemConfig.masterRoot;
    }

    /**
     * @return the appFoler
     */
    public String getAppFoler() {
        return appFoler;
    }

    /**
     * Set application folder.
     */
    public void setAppFoler() {
        this.appFoler = SystemConfig.appFoler;
    }

    /**
     * @return the crSlavePort
     */
    public int getCrSlavePort() {
        return crSlavePort;
    }

    /**
     * Set current slave port.
     */
    public void setCrSlavePort() {
        this.crSlavePort = SystemConfig.crSlavePort;
    }

    /**
     * @return the slotBasePort
     */
    public int getSlotBasePort() {
        return slotBasePort;
    }

    /**
     * Set slot base port.
     */
    public void setSlotBasePort() {
        this.slotBasePort = SystemConfig.slotBasePort;
    }

    /**
     * @return the maxWorkerCount
     */
    public int getMaxWorkerCount() {
        return maxWorkerCount;
    }

    /**
     * Set max worker count.
     */
    public void setMaxWorkerCount() {
        this.maxWorkerCount = SystemConfig.maxWorkerCount;
    }

    /**
     * @return the uiPort
     */
    public int getUiPort() {
        return uiPort;
    }

    /**
     * Set UI port.
     */
    public void setUiPort() {
        this.uiPort = SystemConfig.uiPort;
    }

    /**
     * System summary to json.
     *
     * @return
     */
    public String toJson() {
        return JsonUtil.beanToJson(this);
    }
}
