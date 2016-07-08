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
package com.iveely.framework.net;

import com.iveely.framework.text.Empty;

import org.apache.commons.lang3.SerializationUtils;
import org.apache.log4j.Logger;

import java.io.Serializable;

/**
 * @author {Iveely Liu}
 */
public class Packet<T extends Serializable> implements Serializable {

  private static final long serialVersionUID = 1L;

  /**
   * Logger.
   */
  private static final Logger logger = Logger.getLogger(Packet.class);

  /**
   * Mime type.
   */
  private int mimeType;

  /**
   * Execute type.
   */
  private int executeType;

  /**
   * The context of packet.
   */
  private T data;

  /**
   * Unknown packet.
   */
  public static Packet getUnknownPacket() {
    Packet unknownPacket = new Packet();
    unknownPacket.setExecuteType(999);
    unknownPacket.setMimeType(999);
    unknownPacket.setData(new Empty());
    return unknownPacket;
  }

  /**
   * @return the executeType
   */
  public int getExecuteType() {
    return executeType;
  }

  /**
   * @param executeType the executeType to set
   */
  public void setExecuteType(int executeType) {
    this.executeType = executeType;
  }

  /**
   * Get mime type.
   *
   * @return the mimeType
   */
  public int getMimeType() {
    return this.mimeType;
  }

  /**
   * Set mime type.
   *
   * @param mimeType the mimeType to set
   */
  public void setMimeType(int mimeType) {
    this.mimeType = mimeType;
  }

  /**
   * Set data with byte[].
   */
  public void setData(T data) {
    this.data = data;
  }

  /**
   * Get data.
   */
  public Object getData() {
    return this.data;
  }

  /**
   * Internet packet to bytes.
   */
  public byte[] toBytes() {
    return SerializationUtils.serialize(this);
  }

  /**
   * Convert bytes to Internet packet.
   */
  public Packet toPacket(byte[] bytes) {
    return SerializationUtils.deserialize(bytes);
  }

  /**
   * Mime type in tranfer.
   *
   * @author {Iveely Liu}
   */
  public enum MimeType {

    INTERGE, DOUBLE, BOOLEAN, STRING, OBJECT, JSON,
  }
}
