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
package com.iveely.computing.api;

import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.config.Paths;
import com.iveely.computing.config.SystemConfig;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.io.IReader;
import com.iveely.computing.io.IWriter;
import com.iveely.computing.task.ReaderTask;
import com.iveely.computing.task.TaskRecorder;
import com.iveely.computing.task.WriterTask;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * Topology Builder.
 *
 * Users to build their own Topology, the main components including IInput and
 * IOutput
 */
public class Topology {

  /**
   * Name of the topology.
   */
  private final String name;
  /**
   * Recorder in topology.
   */
  private final TaskRecorder recorder;
  /**
   * User config.
   */
  private final HashMap<String, Object> userConfig;
  /**
   * Execute type.
   */
  private final ExecuteType executeType;
  /**
   * Total count of worker.
   */
  private int workerCount;

  /**
   * Total count of slave.
   */
  private int slaveCount;

  /**
   * Build topology builder.
   *
   * @param type          Specify execution type.
   * @param topologyClass Specify the implementation class of toplogy.
   * @param name          Name of the toplogy.
   */
  public Topology(ExecuteType type, String topologyClass, String name) {
    this.userConfig = new HashMap<>();
    this.workerCount = 0;
    this.name = name;
    this.recorder = new TaskRecorder();
    if (type == ExecuteType.CLUSTER) {
      Coordinator.getInstance().setNodeValue(Paths.getTopologyMapper(topologyClass), topologyClass.replace("$", "."));
    } else {
      SystemConfig.isCluster = false;
    }
    this.executeType = type;
  }

  /**
   * @return the executeType
   */
  public ExecuteType getExecuteType() {
    return executeType;
  }

  /**
   * Get topology name.
   */
  public String getName() {
    return this.name;
  }

  /**
   * Set count of slave if need.
   */
  public void setSlave(int count) {
    this.slaveCount = count;
  }

  /**
   * Return count of slave.
   */
  public int getSlaveCount() {
    int allSlaveCount = Coordinator.getInstance().getChildren(ConfigWrapper.get().getSlave().getRoot()).size();
    if (this.slaveCount < 1 || this.slaveCount > allSlaveCount) {
      this.slaveCount = allSlaveCount / 2 + 1;
    }
    return this.slaveCount;
  }

  /**
   * Get config of user.
   */
  public HashMap<String, Object> getUserConfig() {
    return userConfig;
  }

  /**
   * Set input with default count.
   *
   * Default count is only one.
   *
   * @param input The implementation class of input.
   */
  public void setInput(Class<? extends IInput> input) throws InstantiationException, IllegalAccessException {
    setInput(input, 1);
  }

  /**
   * Set input rader.
   *
   * @param input  The implementation class of IInputReader.
   * @param reader The implementation class of IReader.
   * @param files  The files to be read by current reader.
   */
  public void setReader(Class<? extends IInputReader> input, Class<? extends IReader> reader, List<String> files) throws InstantiationException, IllegalAccessException {
    this.userConfig.put(input.getName(), new ReaderTask(reader.newInstance(), files));
  }

  /**
   * Set output writer.
   *
   * @param output The implementation class of IOutputWriter..
   * @param writer The implementation class of IWriter.
   * @param file   The file to write.
   */
  public void setWriter(Class<? extends IOutputWriter> output, Class<? extends IWriter> writer, String file) throws InstantiationException, IllegalAccessException {
    this.userConfig.put(output.getName(), new WriterTask(writer.newInstance(), file));
  }

  /**
   * Set input workers.
   *
   * @param input       The data input source.
   * @param workerCount The paln to use how many worker.
   */
  public void setInput(Class<? extends IInput> input, int workerCount) throws InstantiationException, IllegalAccessException {
    for (int i = 0; i < workerCount; i++) {
      this.recorder.addInput(input.newInstance());
    }
    this.workerCount += workerCount;
  }

  /**
   * Set output workers.
   */
  public void setOutput(Class<? extends IOutput> output, int workerCount) throws InstantiationException, IllegalAccessException {
    for (int i = 0; i < workerCount; i++) {
      this.recorder.addOutout(output.newInstance());
    }
    this.workerCount += workerCount;
  }

  /**
   * Config information.
   */
  public void put(String key, Object val) {
    userConfig.put(key, val);
  }

  /**
   * Get total count of workers.
   *
   * @return Worker count.
   */
  public int getTotalWorkerCount() {
    return this.workerCount;
  }

  /**
   * Get all inputs.
   *
   * @return Inputs list.
   */
  public List<IInput> getInputs() {
    return this.recorder.getInputs();
  }

  /**
   * Get all outputs.
   *
   * @return Outputs list.
   */
  public List<IOutput> getOutputs() {
    return this.recorder.getOutputs();
  }

  /**
   * Get all names of inputs.
   *
   * @return All names.
   */
  public List<String> getAllputs() {
    List<String> list = new ArrayList<>();
    getInputs().stream().forEach((input) -> {
      list.add(input.getClass().getName());
    });
    getOutputs().stream().forEach((output) -> {
      list.add(output.getClass().getName());
    });
    return list;
  }

  /**
   * Execute type of topology.
   */
  public enum ExecuteType {

    /**
     * Local mode, the purpose for local debugging.
     */
    LOCAL,
    /**
     * Cluster mode,Used for the online production.
     */
    CLUSTER
  }
}
