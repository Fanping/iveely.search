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
package com.iveely.computing.component;

import com.iveely.computing.api.IInput;
import com.iveely.computing.api.StreamChannel;
import com.iveely.computing.config.Paths;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.node.Communicator;

import org.apache.log4j.Logger;

import java.util.HashMap;

/**
 * The executor of the input.
 */
public class InputExecutor extends IExecutor implements Runnable {

  private static final Logger logger = Logger.getLogger(InputExecutor.class);

  /**
   * Input worker.
   */
  private final IInput _input;

  /**
   * Slot flag.
   */
  private final Communicator.Slot _slot;

  /**
   * Build input executor.
   *
   * @param name  Name of the input.
   * @param input Input instance.
   * @param conf  The User-defined configuration information.
   */
  public InputExecutor(String name, IInput input, HashMap<String, Object> conf) {
    this._input = input;
    this._conf = conf;
    this._name = name;
    this._collector = new StreamChannel(_input.getName(), name, this);
    this._slot = Communicator.getInstance().selectSlot();
    Coordinator.getInstance().setNodeValue(Paths.getTopologyInputRecord(this._name, this._input.getClass().getName(), this._input.getName()),
        this._slot.getConnectString());
  }

  /**
   * Start to execute.
   */
  @Override
  public void run() {
    Thread.currentThread().setName(this._input.getName() + " thread");
    try {
      this._input.start(this._conf);
      this._input.toOutput(this._collector);
      while (!this._collector.isTransmitted()) {
        this._input.nextTuple(this._collector);
      }
      this._input.end(this._conf);
    } catch (Exception e) {
      logger.error("Input executor exception happend.", e);
      StringBuilder error = new StringBuilder();
      error.append(e.getMessage());
      error.append("<br/>");
      StackTraceElement[] trace = e.getStackTrace();
      for (StackTraceElement s : trace) {
        error.append("   ");
        error.append(s);
        error.append("<br/>");
      }
      Coordinator.getInstance().setNodeValue(Paths.getTopologyInputStatus(this._name, this._input.getName(), this._slot.getConnectString()), error.toString());
      Coordinator.getInstance().setNodeValue(Paths.getTopologyFinished(this._name), -1 + "");
      Coordinator.getInstance().setNodeValue(Paths.getTopologyStopped(this._name, this._input.getName()), "Exception stopped.");
    }
  }

  /**
   * Stop action.
   */
  public void stop() {
    try {
      this._collector.emitEnd();
    } catch (Exception e) {
      logger.error("When stop tasks in input executor exception happend.", e);
    }
  }
}
