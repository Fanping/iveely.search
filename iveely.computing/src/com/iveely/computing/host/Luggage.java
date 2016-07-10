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
package com.iveely.computing.host;

import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;

/**
 * @author Iveely Liu
 */
public class Luggage {

  /**
   * Slaves sorted by performance.
   */
  public final static List<String> performanceSlaves = new ArrayList<>();

  /**
   * All slaves.
   */
  public final static TreeMap<String, Integer> slaves = new TreeMap<>();

  private Luggage() {
  }

}
