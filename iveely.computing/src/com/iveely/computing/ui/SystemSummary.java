/*
 * Copyright 2016 liufanping@iveely.com.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.iveely.computing.ui;

import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.config.SystemConfig;
import com.iveely.framework.text.JSONUtil;

/**
 * System summary.
 *
 * @author Iveely Liu
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
    private String masterRoot = "/iveely.computing/master";

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

    /**
     * Get system summary.
     */
    public void get() {
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
        this.zkServer = ConfigWrapper.get().getZookeeper().getAddress();
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
        this.zkPort = ConfigWrapper.get().getZookeeper().getPort();
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
        this.masterServer = ConfigWrapper.get().getMaster().getAddress();
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
        this.masterPort = ConfigWrapper.get().getMaster().getPort();
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
        this.slotCount = ConfigWrapper.get().getSlave().getSlotCount();
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
        this.slaveRoot = ConfigWrapper.get().getSlave().getRoot();
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
        this.masterRoot = ConfigWrapper.get().getMaster().getRoot();
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
        this.crSlavePort = ConfigWrapper.get().getSlave().getPort();
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
        this.slotBasePort = ConfigWrapper.get().getSlave().getSlot();
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
        this.uiPort = ConfigWrapper.get().getMaster().getUi_port();
    }

    /**
     * System summary to JSON.
     *
     * @return JSON data.
     */
    public String toJson() {
        return JSONUtil.toString(this);
    }
}
