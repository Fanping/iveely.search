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
 * Master configuration.
 */
public class MasterConfig {

    /**
     * The address of master.
     */
    private String address;

    /**
     * The service port on master.
     */
    private Integer port;

    /**
     * The password when visit UI.
     */
    private String password;

    /**
     * The ui service port.
     */
    private Integer ui_port;

    /**
     * Thes information record position in the zookeeper.
     */
    private String root;

    /**
     * Build instance.
     */
    public MasterConfig() {

    }

    /**
     * Build instance.
     *
     * @param address Address of master.
     * @param port The master service port.
     * @param uiport The ui port.
     * @param password The ui password.
     * @param root The root path on zookeeper to record master information.
     */
    public MasterConfig(String address, Integer port, Integer uiport, String password, String root) {
        this.address = address;
        this.port = port;
        this.password = password;
        this.ui_port = uiport;
        this.root = root;
    }

    /**
     * @return the address
     */
    public String getAddress() {
        return address;
    }

    /**
     * @param address the address to set
     */
    public void setAddress(String address) {
        this.address = address;
    }

    /**
     * @return the port
     */
    public Integer getPort() {
        return port;
    }

    /**
     * @return the password
     */
    public String getPassword() {
        return password;
    }

    /**
     * @param password the password to set
     */
    public void setPassword(String password) {
        this.password = password;
    }

    /**
     * @return the ui_port
     */
    public Integer getUi_port() {
        return ui_port;
    }

    /**
     * @param ui_port the ui_port to set
     */
    public void setUi_port(Integer ui_port) {
        this.ui_port = ui_port;
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
     * @param port the port to set
     */
    public void setPort(Integer port) {
        this.port = port;
    }
}
