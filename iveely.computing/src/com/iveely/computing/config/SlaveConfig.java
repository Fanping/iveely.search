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
 * Slave configuration.
 */
public class SlaveConfig {

    /**
     * The root path on zookeeper to record slave information.
     */
    private String root;

    /**
     * The slave service port.
     */
    private Integer port;

    /**
     * The slot based port.
     */
    private Integer slot;

    /**
     * The slot count to provide services.
     */
    private Integer slotCount;

    /**
     * Build instance.
     */
    public SlaveConfig() {

    }

    /**
     * Build slave configiration.
     *
     * @param port The port that slave based on.
     * @param slot The slot based port.
     * @param slotCount The slot count.
     * @param root The root path on zookeeper to record slave information.
     */
    public SlaveConfig(Integer port, Integer slot, Integer slotCount, String root) {
        this.slot = slot;
        this.port = port;
        this.root = root;
        this.slotCount = slotCount;
    }

    /**
     * @return the port
     */
    public Integer getPort() {
        return port;
    }

    /**
     * @param port the port to set
     */
    public void setPort(Integer port) {
        this.port = port;
    }

    /**
     * @return the slot
     */
    public Integer getSlot() {
        return slot;
    }

    /**
     * @return the root
     */
    public String getRoot() {
        return root;
    }

    /**
     * @param root the root to set
     */
    public void setRoot(String root) {
        this.root = root;
    }

    /**
     * @param slot the slot to set
     */
    public void setSlot(Integer slot) {
        this.slot = slot;
    }

    /**
     * @return the slotCount
     */
    public Integer getSlotCount() {
        return slotCount;
    }

    /**
     * @param slotCount the slotCount to set
     */
    public void setSlotCount(Integer slotCount) {
        this.slotCount = slotCount;
    }

}
