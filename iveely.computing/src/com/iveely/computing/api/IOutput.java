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

import org.apache.log4j.Logger;

import java.util.HashMap;
import java.util.UUID;

/**
 * The basic data output interface.
 *
 * IOutput data sources can be from IOutput and IInput, but the output must
 * IOutput or no output, cannot be directly output to a file.It is the middle of
 * the data processing unit.
 */
public abstract class IOutput implements Cloneable {

  /**
   * Logger.
   */
  private static final Logger logger = Logger.getLogger(IOutput.class.getName());
  /**
   * Unique instance name.
   */
  private final String name;

  /**
   * Build IOutput instance.
   */
  public IOutput() {
    this.name = this.getClass().getSimpleName() + "(" + UUID.randomUUID().toString() + ")";
  }

  /**
   * Prepare before execute, initialization method. You can customize
   * functionality. Including initialization, can be regarded as a constructor.
   *
   * @param conf The user's custom configuration information.
   */
  public void start(HashMap<String, Object> conf) {
  }

  /**
   * Process recived tuple.
   *
   * @param tuple   Data tuple.
   * @param channel Stream channel.
   */
  public abstract void execute(DataTuple tuple, StreamChannel channel);

  /**
   * Data to which output. The next step in the data transfer to another IOuput.
   * If currently IOutput is the final one, you do not need to override this
   * method.
   *
   * @param channel Stream channel.
   */
  public void toOutput(StreamChannel channel) {
  }

  /**
   * Finally function after the execute() method, can be regarded as the
   * destructor.
   *
   * @param conf The user's custom configuration information.
   */
  public void end(HashMap<String, Object> conf) {
  }

  /**
   * @return Name of the task.
   */
  public String getName() {
    return name;
  }
}
