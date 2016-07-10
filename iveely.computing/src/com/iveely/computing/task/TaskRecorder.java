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
package com.iveely.computing.task;

import com.iveely.computing.api.IInput;
import com.iveely.computing.api.IOutput;

import java.util.ArrayList;
import java.util.List;

/**
 * Record every task of topology information,the main is a set of input and
 * Output information.
 *
 * @author Iveely Liu
 */
public class TaskRecorder {

  /**
   * All input tasks.
   */
  private final List<IInput> inputs;

  /**
   * All output tasks.
   */
  private final List<IOutput> outputs;

  /**
   * Build task recorder.
   */
  public TaskRecorder() {
    this.inputs = new ArrayList<>();
    this.outputs = new ArrayList<>();
  }

  /**
   * Add many inputs.
   *
   * @param inputs All inputs.
   */
  public void addInputs(List<IInput> inputs) {
    this.inputs.addAll(inputs);
  }

  /**
   * Add an input.
   *
   * @param input The input.
   */
  public void addInput(IInput input) {
    this.inputs.add(input);
  }

  /**
   * Get all inputs.
   *
   * @return All inputs.
   */
  public List<IInput> getInputs() {
    return this.inputs;
  }

  /**
   * Get count of all inputs.
   *
   * @return Count.
   */
  public int getInputCount() {
    return this.inputs.size();
  }

  /**
   * Add many outputs.
   *
   * @param outputs The outputs.
   */
  public void addOutputs(List<IOutput> outputs) {
    this.outputs.addAll(outputs);
  }

  /**
   * Add an output.
   *
   * @param output The output.
   */
  public void addOutout(IOutput output) {
    this.outputs.add(output);
  }

  /**
   * Get all outputs.
   *
   * @return All outputs.
   */
  public List<IOutput> getOutputs() {
    return this.outputs;
  }

  /**
   * Get count of all outputs.
   *
   * @return Count.
   */
  public int getOutputCount() {
    return this.outputs.size();
  }
}
