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

import com.iveely.framework.text.JSONUtil;

import java.io.File;

/**
 * The wrapper of system configuration.
 */
public class ConfigWrapper {

  private static Configurator configurator;

  private ConfigWrapper() {
  }

  /**
   * Access to the configuration details.
   *
   * @return Instance of the configurator.
   */
  public static Configurator get() {
    if (configurator == null) {
      synchronized (Configurator.class) {
        if (configurator == null) {
          load();
        }
      }
    }
    return configurator;
  }

  /**
   * load configuration information from a file,if load fails,use the default
   * configuration.
   */
  private static void load() {
    Configurator instance = JSONUtil.fromFile(new File("conf/system.json"), Configurator.class);
    if (instance != null) {
      configurator = instance;
    } else {
      configurator = new Configurator();
      configurator.setMaster(new MasterConfig("127.0.0.1", 8001, 9000, "", "/iveely.computing/master"));
      configurator.setSlave(new SlaveConfig(4000, 6000, 6, "/iveely.computing/slave"));
      configurator.setZookeeper(new ZookeeperConfig("127.0.0.1", 2181));
      JSONUtil.toFile(configurator, new File("conf/system.json"));
    }
  }
}
