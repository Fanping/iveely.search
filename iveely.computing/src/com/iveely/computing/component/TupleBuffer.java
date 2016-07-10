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

import com.iveely.computing.common.StreamPacket;

import org.apache.commons.lang3.SerializationUtils;

import java.util.ArrayList;
import java.util.Map;
import java.util.TreeMap;

/**
 * Tuple cache buffer.
 */
public class TupleBuffer {

  /**
   * Cache list.
   */
  private final Map<Integer, ArrayList<StreamPacket>> list;

  /**
   * Max buffer size for each key. When reach max size will be send out.
   */
  private final Integer MAX_BUFFER_SIZE = 10;

  /**
   * Build tuple buffer instance.
   */
  public TupleBuffer() {
    this.list = new TreeMap<>();
  }

  /**
   * Push data intp buffer.
   *
   * @param index  Index of client to send.
   * @param packet packet to prepare send.
   */
  public void push(int index, StreamPacket packet) {
    if (this.list.containsKey(index)) {
      this.list.get(index).add(packet);
    } else {
      ArrayList<StreamPacket> packets = new ArrayList<>();
      packets.add(packet);
      this.list.put(index, packets);
    }
  }

  /**
   * @return Max cached size.
   */
  public int getMaxStored() {
    return this.MAX_BUFFER_SIZE;
  }

  /**
   * Is full on one client.
   *
   * @param index Index of client.
   * @return True is full,or is not.
   */
  public boolean isFull(int index) {
    if (!this.list.containsKey(index)) {
      return false;
    }
    return this.list.get(index).size() >= MAX_BUFFER_SIZE;
  }

  /**
   * Is empty of cahce.
   *
   * @param index Index of client.
   * @return True is empty.
   */
  public boolean isEmpty(int index) {
    if (!this.list.containsKey(index)) {
      return true;
    }
    return this.list.get(index).isEmpty();
  }

  /**
   * Pop cache to bytes.
   *
   * @param index Index of client.
   * @return Bytes data.
   */
  public byte[] pop(int index) {
    if (isEmpty(index)) {
      return new byte[0];
    }
    byte[] ret = SerializationUtils.serialize(this.list.get(index));
    this.list.remove(index);
    return ret;
  }
}
