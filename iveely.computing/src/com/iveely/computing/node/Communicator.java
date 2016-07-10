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

import com.iveely.computing.common.IStreamCallback;
import com.iveely.computing.common.StreamPacket;
import com.iveely.computing.common.StreamType;
import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.framework.net.AsynServer;
import com.iveely.framework.net.Internet;
import com.iveely.framework.net.Packet;
import com.iveely.framework.text.Convertor;

import org.apache.commons.lang3.SerializationUtils;
import org.apache.log4j.Logger;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * Data communicator.
 */
public class Communicator {

  private static final Logger logger = Logger.getLogger(Communicator.class);
  /**
   * Single instance.
   */
  private static Communicator instance;
  /*
   * All slots.
   */
  private final List<Slot> slots;

  /**
   * Thread pool.
   */
  private final ExecutorService threadPool;

  private Communicator() {
    this.slots = new ArrayList<>();
    int slotCount = ConfigWrapper.get().getSlave().getSlotCount();
    int port = ConfigWrapper.get().getSlave().getPort();
    String slaveRoot = ConfigWrapper.get().getSlave().getRoot();
    this.threadPool = Executors.newFixedThreadPool(slotCount);
    for (int i = 0; i < slotCount; i++) {
      Slot slot = new Slot(i);
      this.slots.add(slot);
      Coordinator.getInstance().setNodeValue(slaveRoot + "/" + Internet.getLocalIpAddress() + ","
          + port + "/slots/" + slot.getName(), slot.getConnectString());
      this.threadPool.execute(slot);
    }
  }

  /**
   * Get Single instance.
   */
  public static Communicator getInstance() {
    if (instance == null) {
      instance = new Communicator();
    }
    return instance;
  }

  /**
   * Get best performace slot.
   */
  public Slot selectSlot() {
    int index = 0;
    int min = Integer.MAX_VALUE;
    for (int i = 0; i < this.slots.size(); i++) {
      if (this.slots.get(i).getUsingCount() < min) {
        index = i;
        min = this.slots.get(i).getUsingCount();
      }
    }
    return this.slots.get(index);
  }

  /**
   * Get all slots.
   */
  public List<Slot> getAllSlots() {
    return this.slots;
  }

  /**
   * Get used slot count.
   */
  public int getUsedSlotCount() {
    int count = 0;
    count = slots.stream().map((slot) -> slot.getUsingCount()).reduce(count, Integer::sum);
    return count;
  }

  /**
   * Close communicator.
   */
  public void close() {
    if (this.threadPool != null) {
      this.threadPool.shutdownNow();
    }
  }

  /**
   * Slot of communicator.
   */
  public static class Slot implements Runnable, AsynServer.IHandler {

    /**
     * Service.
     */
    private final AsynServer server;
    /**
     * Get name of the slot.
     */
    private final String name;
    /**
     * All stream callback.
     */
    private final HashMap<String, IStreamCallback> callbacks;
    /**
     * Thread pool.
     */
    private final HashMap<String, ExecutorService> threadPool;
    /**
     * Port of the slot.
     */
    private final int port;
    /**
     * Current in using count.
     */
    private int usingCount;

    /**
     * Build slot.
     *
     * @param port Specify the port for slot to run.
     */
    public Slot(int port) {
      this.port = ConfigWrapper.get().getSlave().getSlot() + port;
      this.server = new AsynServer(this.port, this);
      this.name = UUID.randomUUID().toString();
      this.callbacks = new HashMap<>();
      this.threadPool = new HashMap<>();// Executors.newFixedThreadPool(100);
      this.usingCount = 0;
    }

    @Override
    public Packet process(Packet data) {
      synchronized (Slot.class) {
        Packet packet = data;
        ArrayList<StreamPacket> spackets = (ArrayList<StreamPacket>) SerializationUtils.deserialize((byte[])packet.getData());
        if (spackets != null && !spackets.isEmpty()) {
          for (StreamPacket spacket : spackets) {
            if (!callbacks.containsKey(spacket.getGuid())) {
              logger.error("Not found the callback in slot which packet guid:" + spacket.getGuid());
              Packet resp = new Packet();
              resp.setExecuteType(StreamType.FAILURE.ordinal());
              return resp;
            }
            IStreamCallback callback = callbacks.get(spacket.getGuid());
            this.threadPool.get(spacket.getGuid()).execute(new Task(callback, spacket));
          }
        }
        Packet resp = new Packet();
        resp.setExecuteType(StreamType.SUCCESS.ordinal());
        resp.setData(Convertor.int2byte(spackets.size()));
        return resp;
      }
    }

    @Override
    public void caught(String exception) {
      throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
    }

    /**
     * Register stream callback.
     */
    public void register(String guid, IStreamCallback callback) {
      logger.info(String.format("%s registe to slot[%d] on communicator.", guid, this.port));
      callbacks.put(guid, callback);
      threadPool.put(guid, Executors.newFixedThreadPool(100));
      this.usingCount++;
    }

    /**
     * Unregister stream callback.
     */
    public void unregister(String guid) {
      if (callbacks.containsKey(guid)) {
        callbacks.remove(guid);
        threadPool.get(guid).shutdown();
        threadPool.remove(guid);
        this.usingCount--;
      }
    }

    /**
     * Get count of slot in using.
     */
    public int getUsingCount() {
      return this.usingCount;
    }

    /**
     * Get connect string.
     */
    public String getConnectString() {
      return com.iveely.framework.net.Internet.getLocalIpAddress() + "," + this.port;
    }

    /**
     * Get name of this slot.
     */
    public String getName() {
      return this.name;
    }

    /**
     * Start service of communicator.
     */
    @Override
    public void run() {
      Thread.currentThread().setName("communicator thread");
      try {
        this.server.open();
      } catch (IOException e) {
        logger.error("When setup communicator,exception happend.", e);
      }
    }

    /**
     * communicat task.
     */
    public class Task implements Runnable {

      /**
       * Callback of communicat.
       */
      private final IStreamCallback callback;

      /**
       * Stream packet.
       */
      private final StreamPacket packet;

      /**
       * Build task instance.
       *
       * @param callback The packet to callback.
       * @param packet   The data packet.
       */
      public Task(IStreamCallback callback, StreamPacket packet) {
        this.callback = callback;
        this.packet = packet;
      }

      /**
       * User callback to process data packet.
       */
      @Override
      public void run() {
        Thread.currentThread().setName("communicator of task thread");
        this.callback.invoke(this.packet);
      }
    }
  }
}
