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
 * Zookeeper configiration.
 */
public class ZookeeperConfig {

  /**
   * The address of zookeeper server.
   */
  private String address;

  /**
   * The port to visit zookeeper.
   */
  private Integer port;

  /**
   * Build instance.
   */
  public ZookeeperConfig() {

  }

  /**
   * Build zookeeper configiration.
   *
   * @param address The address of zookeeper.
   * @param port    The port of zookeeper to visit.
   */
  public ZookeeperConfig(String address, Integer port) {
    this.address = address;
    this.port = port;
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

}
