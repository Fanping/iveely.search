package com.iveely.computing.common;

import java.io.Serializable;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class SimpleInteger implements Serializable {

  private int value;

  public SimpleInteger(int value) {
    this.value = value;
  }

  public int getValue() {
    return value;
  }
}
