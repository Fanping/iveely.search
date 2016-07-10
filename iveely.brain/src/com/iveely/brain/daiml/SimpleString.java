package com.iveely.brain.daiml;

import java.io.Serializable;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class SimpleString implements Serializable {

  private String value;

  public SimpleString(String value) {
    this.value = value;
  }

  public String getValue() {
    return value;
  }
}
