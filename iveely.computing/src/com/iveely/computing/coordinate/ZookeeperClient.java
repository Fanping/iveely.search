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
package com.iveely.computing.coordinate;

import com.iveely.computing.common.Message;
import com.iveely.computing.config.ConfigWrapper;

import org.apache.log4j.Logger;
import org.apache.zookeeper.CreateMode;
import org.apache.zookeeper.KeeperException;
import org.apache.zookeeper.WatchedEvent;
import org.apache.zookeeper.Watcher;
import org.apache.zookeeper.Watcher.Event.KeeperState;
import org.apache.zookeeper.ZooDefs;
import org.apache.zookeeper.ZooKeeper;
import org.apache.zookeeper.ZooKeeper.States;

import java.io.IOException;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.concurrent.CountDownLatch;

/**
 * @author Iveely Liu
 */
public class ZookeeperClient implements ICoordinate {

  /**
   * Logger.
   */
  private static final Logger logger = Logger.getLogger(ZookeeperClient.class.getName());
  private static ZookeeperClient client;
  private final ZooKeeper zk;

  private ZookeeperClient() throws IOException {
    CountDownLatch connectedLatch = new CountDownLatch(1);
    Watcher watcher = new ConnectedWatcher(connectedLatch);
    zk = new ZooKeeper(ConfigWrapper.get().getZookeeper().getAddress() + ":" + ConfigWrapper.get().getZookeeper().getPort(),
        10000, watcher);
    waitUntilConnected(zk, connectedLatch);
  }

  /**
   * Wait conect.
   */
  public static void waitUntilConnected(ZooKeeper zooKeeper, CountDownLatch connectedLatch) {
    if (States.CONNECTING == zooKeeper.getState()) {
      try {
        connectedLatch.await();
      } catch (InterruptedException e) {
        throw new IllegalStateException(e);
      }
    }
  }

  /**
   * @return Instance of zookeeper client.
   */
  public static ZookeeperClient getInstance() {
    if (client == null) {
      try {
        client = new ZookeeperClient();
      } catch (Exception e) {
        logger.error("When initialize zookeeper client,exception happend.", e);
      }
    }
    return client;
  }

  /**
   * Get value of a path.
   */
  @Override
  public String getNodeValue(String path) {
    checkNodeExist(path);
    try {
      byte[] data = zk.getData(path, false, null);
      return Message.getString(data);
    } catch (KeeperException | InterruptedException e) {
      logger.error("When get node value on zookeeper,exception happend.", e);
    }
    return "";
  }

  /**
   * Set value of a path.
   */
  @Override
  public void setNodeValue(String path, String val) {
    try {
      checkNodeExist(path);
      if (zk.exists(path, false) != null) {
        zk.setData(path, Message.getBytes(val), -1);
      } else {
        zk.create(path, Message.getBytes(val),
            ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
      }
    } catch (KeeperException | InterruptedException e) {
      logger.error("When set node value on zookeeper,exception happend,", e);
    }
  }

  /**
   * Delete a path.
   */
  @Override
  public void deleteNode(String path) {
    List<String> list = getChildren(path);
    list.stream().map((list1) -> (path + "/" + list1).replace("//", "/")).forEach((childPath) -> {
      deleteNode(childPath);
    });
    try {
      if (zk.exists(path, false) != null) {
        zk.delete(path, -1);
      }
    } catch (KeeperException | InterruptedException ex) {
      //logger.error("Delete node exception.", ex);
    }
  }

  /**
   * Get child nodes of a path.
   */
  @Override
  public List<String> getChildren(String path) {
    List<String> children = new ArrayList<>();
    path = path.replace("//", "/");
    checkNodeExist(path);
    try {
      if (zk.exists(path, null) != null) {
        children = zk.getChildren(path, false);
      }
    } catch (KeeperException | InterruptedException e) {
      logger.error("When get children path on zookeeper,exception happend.", e);
    }
    return children;
  }

  /**
   * Check node is ready.
   */
  @Override
  public void checkNodeExist(String path) {
    try {
      String[] paths = path.split("/");
      String tempPath = "";
      for (int i = 0; i < paths.length - 1; i++) {
        tempPath = tempPath + "/" + paths[i];
        tempPath = tempPath.replaceAll("//", "/");
        if (zk.exists(tempPath, null) == null) {
          zk.create(tempPath, new Date().toString().getBytes(Charset.defaultCharset()), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
        }

      }
    } catch (KeeperException | InterruptedException e) {
      logger.error("When check node is exist on zookeeper,exception happend.", e);
    }
  }

  static class ConnectedWatcher implements Watcher {

    private final CountDownLatch connectedLatch;

    ConnectedWatcher(CountDownLatch connectedLatch) {
      this.connectedLatch = connectedLatch;
    }

    @Override
    public void process(WatchedEvent event) {
      if (event.getState() == KeeperState.SyncConnected) {
        connectedLatch.countDown();
      }
      if (event.getState() == KeeperState.Expired) {
        try {
          logger.info("Zookeeper session expired.");
          client = new ZookeeperClient();
        } catch (IOException ex) {
          logger.error("Zookeeper client exception on process.", ex);
        }
      }
    }
  }

}
