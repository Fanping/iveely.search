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
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.host.Luggage;

import org.apache.log4j.Logger;

import java.util.Iterator;

/**
 * Cluster information summary.
 */
public class ClusterSummary {

  /**
   * Logger
   */
  private final Logger logger = Logger.getLogger(ClusterSummary.class.getName());

  /**
   * Response type.
   */
  private String resType;
  /**
   * Current version of computing.
   */
  private String version;
  private String setupTime;
  /**
   * Count of slaves.
   */
  private int slaveCount;
  /**
   * Total slotsCount.
   */
  private int totalSlotCount;
  /**
   * Count of used slots.
   */
  private int usedSlotCount;
  /**
   * Count of free slots.
   */
  private int freeSlotCount;

  /**
   * @return Get response type of cluster summary, actually is "cluster
   * summary".
   */
  public String getResType() {
    return this.resType;
  }

  /**
   * @return the version
   */
  public String getVersion() {

    return version;
  }

  /**
   * @return the setupTime
   */
  public String getSetupTime() {

    return this.setupTime;
  }

  /**
   * @return the slaveCount
   */
  public int getSlaveCount() {
    return slaveCount;
  }

  /**
   * @return Get total slot count.
   */
  public int getTotalSlotCount() {
    return this.totalSlotCount;
  }

  /**
   * @return Get used slot count.
   */
  public int getUsedSlotCount() {
    return this.usedSlotCount;
  }

  /**
   * @return Get free slot count.
   */
  public int getFreeSlotCount() {
    return this.freeSlotCount;
  }

  /**
   * Get cluster summary information.
   */
  public void get() {

    // 0. Type
    this.resType = "cluster summary";

    // 1. Version
    if (this.version == null || this.version.isEmpty()) {
      this.version = ClusterSummary.class.getPackage().getSpecificationVersion();
    }

    // 2. Master uptime
    if (this.setupTime == null || this.setupTime.isEmpty()) {
      String time = Coordinator.getInstance().getNodeValue(ConfigWrapper.get().getMaster().getRoot() + "/setup");
      this.setupTime = time;
    }

    // 3. Count of slaves.
    this.slaveCount = Coordinator.getInstance().getChildren(ConfigWrapper.get().getSlave().getRoot()).size();

    // 4. Total slots.
    this.totalSlotCount = this.slaveCount * ConfigWrapper.get().getSlave().getSlotCount();

    // 5. Used slots.
    this.usedSlotCount = 0;
    Iterator it = Luggage.slaves.keySet().iterator();
    while (it.hasNext()) {
      this.usedSlotCount += Luggage.slaves.get(it.next());
    }

    // 6. Free slots.
    this.freeSlotCount = this.totalSlotCount - this.usedSlotCount;
  }
}
