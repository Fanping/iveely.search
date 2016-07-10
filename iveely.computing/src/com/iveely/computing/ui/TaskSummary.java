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

import java.util.ArrayList;
import java.util.List;

/**
 * Task summary information.
 */
public class TaskSummary {

  /**
   * Response type.
   */
  private String resType;
  /**
   * Buffer of child.
   */
  private String zBuffer;

  /**
   * @return Get response type,actually is "task summary".
   */
  public String getResType() {
    return this.resType;
  }

  /**
   * Get buffer.
   *
   * @return JSON buffer.
   */
  public String getZBuffer() {
    return this.zBuffer;
  }

  /**
   * Get task summary information by topology.
   *
   * @param tpName Name of the topology.
   */
  public void get(String tpName) {

    // 1. type.
    this.resType = "task summary";

    // 2. Get all task (Input & Output)
    this.zBuffer = "[";
    List<String> taskType = new ArrayList<>();
    taskType.add("input");
    taskType.add("output");
    taskType.stream().forEach((tt) -> {
      List<String> inputOrOutputs = new ArrayList<>();
      List<String> allClasses = Coordinator.getInstance().getChildren(Paths.getTopologyRoot(tpName) + "/" + tt);
      allClasses.stream().forEach((clas) -> {
        List<String> temp = Coordinator.getInstance().getChildren(Paths.getTopologyRoot(tpName) + "/" + tt + "/" + clas
        );
        if (temp.size() > 0) {
          temp.stream().forEach((t) -> {
            inputOrOutputs.add(clas + "/" + t);
          });
        }
      });
      inputOrOutputs.stream().map((inputOrOutput) -> {
        String[] simDetail = inputOrOutput.split("/");
        TaskDetail task = new TaskDetail();
        task.setGuid(simDetail[1]);
        task.setType(tt);
        String connect = Coordinator.getInstance()
            .getNodeValue(Paths.getTopologyRoot(tpName) + tt + "/" + inputOrOutput);
        task.setConnectPath(connect);
        List<String> exceptionSlots = Coordinator.getInstance()
            .getChildren(Paths.getTopologyStatusRoot(tpName) + "/" + tt + "/" + simDetail[1]);
        if (exceptionSlots.isEmpty()) {
          task.setException(" ");
        } else {
          String excpt = Coordinator.getInstance().getNodeValue(
              Paths.getTopologyRoot(tpName) + "/" + tt + "/" + simDetail[1] + "/" + exceptionSlots.get(0));
          task.setException(excpt);
        }
        return task;
      }).forEach((task) -> {
        this.zBuffer += task.toJson() + ",";
      });
    });
    this.zBuffer = this.zBuffer.substring(0, this.zBuffer.length() - 1);
    this.zBuffer += "]";
  }

  /**
   * Task summary to json.
   */
  public String toJson() {
    return JSONUtil.toString(this);
  }

  /**
   * Task detail information.
   */
  public static class TaskDetail {

    /**
     * Guid of the inputOrOutput.
     */
    private String guid;
    /**
     * Connect net path.
     */
    private String connectPath;
    /**
     * Exception on this task.
     */
    private String exception;
    private String type;

    /**
     * @return the guid
     */
    public String getGuid() {
      return guid;
    }

    /**
     * @param guid the guid to set
     */
    public void setGuid(String guid) {
      this.guid = guid;
    }

    /**
     * @return the connectPath
     */
    public String getConnectPath() {
      return connectPath;
    }

    /**
     * @param connectPath the connectPath to set
     */
    public void setConnectPath(String connectPath) {
      this.connectPath = connectPath;
    }

    /**
     * @return the exception
     */
    public String getException() {
      return exception;
    }

    /**
     * @param exception the exception to set
     */
    public void setException(String exception) {
      this.exception = exception;
    }

    /**
     * @return JSON data of serilize this.
     */
    public String toJson() {
      return JSONUtil.toString(this);
    }

    /**
     * @return the type
     */
    public String getType() {
      return type;
    }

    /**
     * @param type the type to set
     */
    public void setType(String type) {
      this.type = type;
    }
  }
}
