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

import com.iveely.computing.config.Paths;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.framework.text.JSONUtil;

import java.util.List;

/**
 * Topology summary.
 */
public class TopologySummary {

  /**
   * Response type.
   */
  private String resType;
  /**
   * All topology simple information.
   */
  private String zBuffer;

  /**
   * @return Get response type,actually is "topology summary".
   */
  public String getResType() {
    return this.resType;
  }

  /**
   * Get buffer.
   */
  public String getZBuffer() {
    return this.zBuffer;
  }

  /**
   * Initialize.
   */
  public void init() {
    // 1. Response type.
    this.resType = "topology summary";

    // 2. Build topologys.
    List<String> names = Coordinator.getInstance().getChildren(Paths.getAppRoot());
    this.zBuffer = "[";
    if (names.size() > 0) {
      names.stream().map((name) -> new TopologySimple(name)).map((simple) -> {
        simple.get();
        return simple;
      }).forEach((simple) -> {
        this.zBuffer += simple.toJson() + ",";
      });
    }
    this.zBuffer = this.zBuffer.substring(0, this.zBuffer.length() - 1);
    this.zBuffer += "]";
  }

  /**
   * Query to json.
   */
  public String queryToJson(String tpName) {
    // 1. Response type.
    this.resType = "query topology";

    // 2. Build topologys.
    List<String> names = Coordinator.getInstance().getChildren(Paths.getAppRoot());
    this.zBuffer = "[";
    if (names.size() > 0) {
      names.stream().filter((name) -> (name.equals(tpName))).map((name) -> new TopologySimple(name))
          .map((simple) -> {
            simple.get();
            return simple;
          }).forEach((simple) -> {
        this.zBuffer += simple.toJson() + ",";
      });
    }
    this.zBuffer = this.zBuffer.substring(0, this.zBuffer.length() - 1);
    this.zBuffer += "]";
    return toJson();
  }

  /**
   * Topology summary to json.
   */
  public String toJson() {
    return JSONUtil.toString(this);
  }

  /**
   * Toplogy simple information.
   */
  public static class TopologySimple {

    /**
     * Name of the topology.
     */
    private final String name;
    /**
     * Id of the topology.
     */
    private final int id;
    /**
     * setupTime of the topology.
     */
    private String setupTime;
    /**
     * Status of topology.
     */
    private String status;
    /**
     * How many slaves run this topology.
     */
    private int inSlaveCount;
    /**
     * How many thread run this topology.
     */
    private int threadCount;

    /**
     * Build topology simple instance.
     *
     * @param name Name of the topology.
     */
    public TopologySimple(String name) {
      this.name = name;
      this.id = name.hashCode();
    }

    /**
     * @return Name of the topology.
     */
    public String getName() {
      return this.name;
    }

    /**
     * @return Get topology id.
     */
    public int getId() {
      return this.id;
    }

    /**
     * @return Get topology setup time.
     */
    public String getSetupTime() {
      return this.setupTime;
    }

    /**
     * @return Get status of toplogy.
     */
    public String getStatus() {
      return this.status;
    }

    /**
     * @return Get number occupies the slaves.
     */
    public int getInSlaveCount() {
      return this.inSlaveCount;
    }

    /**
     * @return Get all thread count.
     */
    public int getThreadCount() {
      return this.threadCount;
    }

    /**
     * Get toplogy summary.
     */
    public void get() {
      // 1. Status.
      int count = Integer.parseInt(Coordinator.getInstance().getNodeValue(Paths.getTopologyFinished(this.name)));
      int finishCount = Coordinator.getInstance().getChildren(Paths.getTopologyFinished(this.name)).size();
      if (count == finishCount && count != -1) {
        this.status = "Completed";
      } else if (count == -1) {
        this.status = "Exception";
      } else {
        this.status = "Running";
      }

      // 2. Setuptime.
      String time = Coordinator.getInstance().getNodeValue(Paths.getTopologyRoot(this.name));
      this.setupTime = time;

      // 3. InSlave
      this.inSlaveCount = Integer
          .parseInt(Coordinator.getInstance().getNodeValue(Paths.getTopologySlaveCount(this.name)));

      // 4. Thread count.
      this.threadCount = Coordinator.getInstance().getChildren(Paths.getTopologyRoot(this.name)).size();
    }

    /**
     * @return JSON serialize this.
     */
    public String toJson() {
      return JSONUtil.toString(this);
    }
  }
}
