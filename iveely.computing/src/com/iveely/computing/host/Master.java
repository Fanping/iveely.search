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

import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.config.SystemConfig;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.ui.UIProvider;
import com.iveely.framework.net.AsynServer;

import org.apache.log4j.Logger;
import org.apache.zookeeper.KeeperException;

import java.io.File;
import java.io.IOException;
import java.util.Date;

/**
 * Master of the cluster.
 *
 * @author Iveely Liu
 */
public class Master implements Runnable {

  /**
   * Event server from slave to master.
   */
  private final AsynServer server;

  /**
   * Event processor from slave to master.
   */
  private final MasterProcessor masterProcessor;

  /**
   * WebSocket provider for UI.
   */
  private final UIProvider uiProvider;

  /**
   * Check node is online.
   */
  private final SignValidator validator;
  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(Master.class.getName());
  /**
   * Thread for UI
   */
  private Thread uiThread;
  private Thread validatorThread;
  /**
   * Has started of master.
   */
  private boolean isStarted;

  /**
   * Build master.
   *
   * @param zookeeperServer The zookeeper server location.
   * @param zookeeperPort   The zookeeper port to visit.
   * @param uiPwd           Set password for ui.
   */
  public Master(String zookeeperServer, int zookeeperPort, String uiPwd) throws IOException, KeeperException, InterruptedException {
    this.validator = new SignValidator();
    this.masterProcessor = new MasterProcessor(this.validator);
    this.uiProvider = new UIProvider(masterProcessor, uiPwd);
    int masterPort = ConfigWrapper.get().getMaster().getPort();
    this.server = new AsynServer(masterPort, masterProcessor);
    this.isStarted = false;
    initZookeeper(zookeeperServer, zookeeperPort, masterPort);
    initFolder();
    initUIService();
    initValidatorService();
  }

  /**
   * Start master.
   */
  @Override
  public void run() {
    Thread.currentThread().setName("master main thread");
    try {
      if (!isStarted) {
        isStarted = server.open();
      }
    } catch (IOException ex) {
      logger.error("fail to start master.", ex);
    }
  }

  /**
   * Init zookeeper.
   */
  private void initZookeeper(String server, int port, int masterPort)
      throws IOException, KeeperException, InterruptedException {

    // 2. Create root\master
    String master = com.iveely.framework.net.Internet.getLocalIpAddress()
        + "," + masterPort;
    String masterRoot = ConfigWrapper.get().getMaster().getRoot();
    Coordinator.getInstance().deleteNode("/");
    Coordinator.getInstance().setNodeValue(masterRoot, master);
    Coordinator.getInstance().setNodeValue(masterRoot + "/setup", new Date().toString());
    logger.info("master information:" + master);
  }

  private void initFolder() {
    File file = new File(SystemConfig.appFoler);
    if (!file.exists()) {
      if (!file.mkdir()) {
        logger.error(SystemConfig.appFoler + " not created on master.");
      }
    }
  }

  /**
   * UI Service.
   */
  private void initUIService() {
    uiThread = new Thread(this.uiProvider);
    uiThread.start();
  }

  /**
   * Validator Service.
   */
  private void initValidatorService() {
    validatorThread = new Thread(this.validator);
    validatorThread.start();
  }

  /**
   * Stop master.
   */
  public void stop() {
    if (uiThread != null && uiThread.isAlive()) {
      uiThread.interrupt();
    }
    if (validatorThread != null && validatorThread.isAlive()) {
      validatorThread.interrupt();
    }
    server.close();
  }
}
