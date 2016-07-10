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

import com.iveely.computing.common.Message;
import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.framework.net.Packet;
import com.iveely.framework.process.ThreadUtil;

import org.apache.log4j.Logger;

import java.util.Date;
import java.util.HashMap;
import java.util.Iterator;

/**
 * Detection node is still alive, signed in validation of slave.
 */
public class SignValidator implements Runnable {

  /**
   * Logger
   */
  private final Logger logger = Logger.getLogger(SignValidator.class.getName());

  /**
   * All slaves of the heart information.
   */
  private HashMap<String, Long> heartbeat;

  /**
   * Master processor.
   */
  private MasterProcessor masterProcessor;

  /**
   * Build sign validator.
   */
  public SignValidator() {
    if (heartbeat == null) {
      heartbeat = new HashMap<>();
    }
  }

  /**
   * Set master processor.
   *
   * @param processor The master processor.
   */
  public void setMasterProcessor(MasterProcessor processor) {
    this.masterProcessor = processor;
  }

  /**
   * Slave's heartbeat comes.
   *
   * @param ipaddress Ipaddress of the slave.
   */
  public void arrive(String ipaddress) {
    heartbeat.put(ipaddress, new Date().getTime());
  }

  /**
   * Start to check node.
   */
  @Override
  public void run() {
    Thread.currentThread().setName("sign validator thread");
    while (true) {
      try {
        Iterator iter = heartbeat.keySet().iterator();
        while (iter.hasNext()) {
          String key = (String) iter.next();
          Long val = heartbeat.get(key);
          long diff = new Date().getTime() - val;
          if (diff / 1000 > 60) {
            Coordinator.getInstance().deleteNode(ConfigWrapper.get().getSlave().getRoot() + "/" + key);
            Luggage.slaves.remove(key);
            Luggage.performanceSlaves.remove(key);
            logger.error(key + " is crashed.");
            logger.info("Check topologies on " + key);
            String[] list = SlaveTopology.getInstance().get(key);
            if (list.length > 0) {
              for (String tpName : list) {
                Packet packet = new Packet();
                packet.setExecuteType(Message.ExecuteType.REBALANCE.ordinal());
                packet.setData(Message.getBytes(tpName));
                packet.setMimeType(Message.MIMEType.MESSAGE.ordinal());
                this.masterProcessor.process(packet);
              }
            } else {
              logger.info("No topologies on this node.");
            }
            iter.remove();
          }
        }
        ThreadUtil.sleep(10);
      } catch (Exception e) {
        logger.error("When validate heartbeat,exception happend.", e);
        logger.error(e);
      }
    }
  }
}
