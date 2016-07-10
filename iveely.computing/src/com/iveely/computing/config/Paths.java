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
 * Storage to the Zookeeper path management.
 */
public class Paths {

  public static String getAppRoot() {
    return "/app";
  }

  /**
   * Path data sources to send the amount of data statistics.
   *
   * @param appName   Application name.
   * @param inputName IInput name.
   * @return Path.
   */
  public static String getInputTotalEmit(String appName, String inputName) {
    return getAppRoot() + "/" + appName + "/statistic/input/" + inputName + "/emit";
  }

  public static String getInputAvgEmit(String appName, String inputName) {
    return getAppRoot() + "/" + appName + "/statistic/input/" + inputName + "/emit_avg";
  }

  public static String getInputEmitFinished(String appName, String inputName) {
    return getAppRoot() + "/" + appName + "/finished/" + inputName;
  }

  public static String getRecordExecutorOutput(String executorName, String outputName) {
    return getAppRoot() + "/" + executorName + "/output/" + outputName;
  }

  public static String getTopologyMapper(String topologyName) {
    return "/iveely.computing/app/mapper/" + topologyName;
  }

  public static String getTopologyRoot(String topologyName) {
    return getAppRoot() + "/" + topologyName;
  }

  public static String getTopologyFinished(String topologyName) {
    return getTopologyRoot(topologyName) + "/finished";
  }

  public static String getTopologySlaveCount(String topologyName) {
    return getTopologyRoot(topologyName) + "/slavecount";
  }

  public static String getTopologyInputRecord(String topologyName, String inputClass, String inputName) {
    return getTopologyRoot(topologyName) + "/input/" + inputClass + "/" + inputName;
  }

  public static String getTopologyOutputRecord(String topologyName, String outputClass, String outputName) {
    return getTopologyRoot(topologyName) + "/output/" + outputClass + "/" + outputName;
  }

  public static String getTopologyStopped(String topologyName, String inputName) {
    return getTopologyFinished(topologyName) + "/" + inputName;
  }

  public static String getTopologyInputStatus(String topologyName, String inputName, String slotName) {
    return getTopologyRoot(topologyName) + "/statistic/input/" + inputName + "/" + slotName;
  }

  public static String getTopologyOutputExecute(String topologyName, String outputName) {
    return getTopologyRoot(topologyName) + "/statistic/output/" + outputName + "/execute";
  }

  public static String getTopologyOutputAvgExecute(String topologyName, String outputName) {
    return getTopologyRoot(topologyName) + "/statistic/output/" + outputName + "/execute_avg";
  }

  public static String getTopologyStatusRoot(String topologyName) {
    return getTopologyRoot(topologyName) + "/status";
  }
}
