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
package com.iveely.computing.node;

import com.iveely.computing.common.Message;
import com.iveely.computing.config.ConfigWrapper;
import com.iveely.framework.net.AsynClient;
import com.iveely.framework.net.Internet;
import com.iveely.framework.net.Packet;
import com.iveely.framework.process.ThreadUtil;
import com.iveely.framework.text.JSONUtil;

import org.apache.log4j.Logger;

/**
 * The heart information, by the slave sent to the master.
 */
public class Heartbeat implements Runnable {

  /**
   * Message client.
   */
  private final AsynClient client;
  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(Heartbeat.class.getName());
  private boolean shouldWork;

  /**
   * Build heartbeat.
   */
  public Heartbeat() {
    client = new AsynClient(ConfigWrapper.get().getMaster().getAddress(), ConfigWrapper.get().getMaster().getPort(), null);
    this.shouldWork = true;
  }

  /**
   * Start heartbeat to prove alive.
   */
  @Override
  public void run() {
    Thread.currentThread().setName("slave heartbeat thread");
    while (this.shouldWork) {
      try {
        Packet packet = new Packet();
        packet.setExecuteType(Message.ExecuteType.HEARTBEAT.ordinal());
        packet.setMimeType(Message.MIMEType.MESSAGE.ordinal());
        packet.setData(Message.getBytes(beatInfo()));
        client.send(packet);
        ThreadUtil.sleep(5);
      } catch (Exception ex) {
        logger.error("Send heartbeat stoped:" + ex.getMessage(), ex.getCause());
        return;
      }
    }
  }

  /**
   * Stop heartbeat.
   */
  public void stop() {
    this.shouldWork = false;
  }

  /**
   * Heartbeat information.
   *
   * @return Heartbeat information.
   */
  private String beatInfo() {
    return new HeartbeatInfo().toString();
  }

  /**
   * Heart beat informations.
   */
  public static class HeartbeatInfo {

    /**
     * The heartbeat from address.
     */
    private String ipaddress;

    /**
     * The heartbeat from port.
     */
    private int port;

    /**
     * The current slave used slot count.
     */
    private int usedSlot;

    /**
     * Build instance.
     */
    public HeartbeatInfo() {
      this.ipaddress = Internet.getLocalIpAddress();
      this.port = ConfigWrapper.get().getSlave().getPort();
      this.usedSlot = Communicator.getInstance().getUsedSlotCount();
    }

    /**
     * @return the ipaddress
     */
    public String getIpaddress() {
      return ipaddress;
    }

    /**
     * @param ipaddress the ipaddress to set
     */
    public void setIpaddress(String ipaddress) {
      this.ipaddress = ipaddress;
    }

    /**
     * @return the port
     */
    public int getPort() {
      return port;
    }

    /**
     * @param port the port to set
     */
    public void setPort(int port) {
      this.port = port;
    }

    /**
     * @return the freeSlot
     */
    public int getUsedSlot() {
      return usedSlot;
    }

    /**
     * @param usedSlot the freeSlot to set
     */
    public void setUsedSlot(int usedSlot) {
      this.usedSlot = usedSlot;
    }

    /**
     * @return JSON String of serilize this.
     */
    @Override
    public String toString() {
      return JSONUtil.toString(this);
    }
  }
}
