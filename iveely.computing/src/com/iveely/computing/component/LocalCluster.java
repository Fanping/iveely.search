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
import com.iveely.computing.api.IOutput;
import com.iveely.computing.api.Topology;
import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.config.Paths;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.host.Master;
import com.iveely.computing.node.Slave;
import com.iveely.framework.process.ThreadUtil;

import org.apache.log4j.Logger;

import java.util.HashMap;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.ThreadPoolExecutor;

/**
 * Local cluster running environment.
 *
 * @author Iveely Liu
 */
public class LocalCluster {

  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(LocalCluster.class.getName());

  /**
   * Total count of workers.
   */
  private int workerCount;

  /**
   * Thread pool.
   */
  private ExecutorService threadPool;

  /**
   * User config.
   */
  private HashMap<String, Object> userConfig;

  /**
   * input config.
   */
  private List<IInput> inputConfig;

  /**
   * output config.
   */
  private List<IOutput> outputConfig;

  private Master master;

  private Slave slave;

  /**
   * Submit to local cluster.
   */
  public void submit(Topology builder) {

    // 0. Init.
    this.workerCount = builder.getTotalWorkerCount();
    this.threadPool = Executors.newFixedThreadPool(3 + workerCount);

    // 1. Run local cluster.
    if (!runCluster()) {
      this.threadPool.shutdownNow();
      return;
    }

    // 2. Build config.
    Coordinator.getInstance().deleteNode(Paths.getTopologyRoot(builder.getName()));
    userConfig = builder.getUserConfig();
    inputConfig = builder.getInputs();
    outputConfig = builder.getOutputs();

    // 3. Try to run output.
    for (int i = outputConfig.size() - 1; i > -1; i--) {
      IOutput output = outputConfig.get(i);
      String name = output.getName();
      logger.info(name);
      OutputExecutor executor = new OutputExecutor(builder.getName(), output, userConfig);
      executor.initialize();
      //threadPool.execute(executor);
    }

    // 4. Try to run input.
    for (int i = inputConfig.size() - 1; i > -1; i--) {
      IInput input = inputConfig.get(i);
      logger.info("run " + input.getName());
      InputExecutor executor = new InputExecutor(builder.getName(), input, userConfig);
      threadPool.execute(executor);
    }

    // 5.Close all threads.
    int threadCount = ((ThreadPoolExecutor) this.threadPool).getActiveCount();
    while ((threadCount = ((ThreadPoolExecutor) this.threadPool).getActiveCount()) > 0) {
      ThreadUtil.sleep(1);
    }
  }

  /**
   * Run local cluster.
   */
  private boolean runCluster() {
    try {
      // 1. Run master.
      logger.info("starting master...");
      master = new Master(ConfigWrapper.get().getZookeeper().getAddress(), ConfigWrapper.get().getZookeeper().getPort(), "");
      threadPool.execute(master);

      // 2. Run slave.
      logger.info("starting slave...");
      slave = new Slave(ConfigWrapper.get().getZookeeper().getAddress(), ConfigWrapper.get().getZookeeper().getPort());
      threadPool.execute(slave);
      return true;
    } catch (Exception e) {
      logger.error("Local cluster setup exception.", e);
    }
    return false;
  }

  /**
   * Stop local cluster.
   */
  public void stop() {
    threadPool.shutdownNow();
    if (slave != null) {
      slave.stop();
    }
    if (master != null) {
      master.stop();
    }
  }
}
