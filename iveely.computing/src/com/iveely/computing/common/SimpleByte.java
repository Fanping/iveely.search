package com.iveely.computing.common;

import java.io.Serializable;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class SimpleByte implements Serializable {

  private byte[] bytes;

  public SimpleByte(byte[] bytes) {
    this.bytes = bytes;
  }

  public byte[] getBytes() {
    return bytes;
  }
}
