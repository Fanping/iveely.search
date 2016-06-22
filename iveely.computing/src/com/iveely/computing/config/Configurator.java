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
package com.iveely.computing.config;

/**
 * Configurator, including system configuration information of each component.
 */
public class Configurator {

    /**
     * Master configuration.
     */
    private MasterConfig master;

    /**
     * Slave configuration.
     */
    private SlaveConfig slave;

    /**
     * Zoojeeper configuration.
     */
    private ZookeeperConfig zookeeper;

    /**
     * Build instance.
     */
    public Configurator() {

    }

    /**
     * @return Get master configuration.
     */
    public MasterConfig getMaster() {
        return master;
    }

    /**
     * @param master the master to set
     */
    public void setMaster(MasterConfig master) {
        this.master = master;
    }

    /**
     * @return Get slave configuration.
     */
    public SlaveConfig getSlave() {
        return slave;
    }

    /**
     * @param slave the slave to set
     */
    public void setSlave(SlaveConfig slave) {
        this.slave = slave;
    }

    /**
     * @return Get zookeeper configuration.
     */
    public ZookeeperConfig getZookeeper() {
        return zookeeper;
    }

    /**
     * @param zookeeper the zookeeper to set
     */
    public void setZookeeper(ZookeeperConfig zookeeper) {
        this.zookeeper = zookeeper;
    }
}
