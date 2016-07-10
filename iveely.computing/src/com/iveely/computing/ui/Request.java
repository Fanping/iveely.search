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

/**
 * Request.
 *
 * @author Iveely Liu
 */
public class Request {

  /**
   * User command.
   */
  private String command;
  /**
   * Name of the topology.
   */
  private String topology;

  /**
   * @return the command
   */
  public String getCommand() {
    return command;
  }

  /**
   * @param command the command to set
   */
  public void setCommand(String command) {
    this.command = command;
  }

  /**
   * @return the topology
   */
  public String getTopology() {
    return topology;
  }

  /**
   * @param topology the topology to set
   */
  public void setTopology(String topology) {
    this.topology = topology;
  }
}
