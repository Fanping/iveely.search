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
package com.iveely.framework.system.net;

import com.iveely.framework.system.text.Convertor;

import org.apache.log4j.Logger;

import java.io.Serializable;

/**
 * @author {Iveely Liu}
 */
public class Packet implements Serializable {

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
  private byte[] data;

  /**
   * Unknown packet.
   */
  public static Packet getUnknowPacket() {
    Packet unknowPacket = new Packet();
    unknowPacket.setExecuteType(999);
    unknowPacket.setMimeType(999);
    unknowPacket.setData(new byte[]{0, 0, 0, 0});
    return unknowPacket;
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
  public void setData(byte[] data) {
    this.data = (byte[]) data.clone();
  }

  /**
   * Get data.
   */
  public byte[] getData() {
    return (byte[]) this.data.clone();
  }

  /**
   * Set data with type of string.
   */
  public void setData(String data) {
    this.data = com.iveely.framework.system.text.StringUtil.getBytes(data);
  }

  public String getDatatoString() {
    return com.iveely.framework.system.text.StringUtil.getString(this.data);
  }

  /**
   * Internet packet to bytes.
   */
  public byte[] toBytes() {
    byte[] bytes = new byte[getData().length + 8];
    byte[] executeTypeBytes = Convertor.int2byte(getExecuteType());
    byte[] mimeTypeBytes = Convertor.int2byte(getMimeType());
    System.arraycopy(executeTypeBytes, 0, bytes, 0, 4);
    System.arraycopy(mimeTypeBytes, 0, bytes, 4, 4);
    System.arraycopy(getData(), 0, bytes, 8, getData().length);
    return bytes;
  }

  /**
   * Convert bytes to Internet packet.
   */
  public Packet toPacket(byte[] bytes) {
    if (bytes.length == 0) {
      // close system cmd.
      return null;
    }
    if (bytes.length < 8) {
      return Packet.getUnknowPacket();
    }
    try {
      byte[] executeTypeBytes = new byte[4];
      System.arraycopy(bytes, 0, executeTypeBytes, 0, 4);
      int exeType = Convertor.bytesToInt(executeTypeBytes);
      setExecuteType(exeType);

      byte[] mimeTypeBytes = new byte[4];
      System.arraycopy(bytes, 4, mimeTypeBytes, 0, 4);
      int mType = Convertor.bytesToInt(mimeTypeBytes);
      setMimeType(mType);

      byte[] dataBytes = new byte[bytes.length - 8];
      System.arraycopy(bytes, 8, dataBytes, 0, dataBytes.length);
      setData(dataBytes);
      return this;
    } catch (Exception e) {
      logger.error(e);
    }
    return getUnknowPacket();
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
