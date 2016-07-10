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

import com.iveely.computing.api.StreamChannel;

import java.util.HashMap;

/**
 * The Executor implementation of interfaces. Include InputExecutor and
 * OutputExecutor.
 *
 * @author Iveely Liu
 */
public class IExecutor {

  /**
   * User-defined configuration information.
   */
  protected HashMap<String, Object> _conf;

  /**
   * Output of the collector.
   */
  protected StreamChannel _collector;

  /**
   * Name of the topology.
   */
  protected String _name;

  /**
   * Get name of the toplogy.
   *
   * @return Name of the topology.
   */
  public String getName() {
    return this._name;
  }
}
