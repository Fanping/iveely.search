package com.iveely.database.type;

import com.iveely.database.common.Common;

import java.io.Serializable;

/**
 * Short string.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 21:47:33
 */
public class ShortString implements Serializable {

  /**
   * The context.
   */
  private String data;

  public ShortString(String data) {
    if (data.length() < Common.getShortStringSize()) {
      this.data = data;
    }
  }

  /**
   * Get default short string.
   */
  public static ShortString getDefaultShortString() {
    try {
      ShortString val = new ShortString(" ");
      return val;
    } catch (Exception e) {
      // Never happen here.
    }
    return null;
  }

  /**
   * Set value.
   */
  public boolean setValue(String data) {
    if (data.length() < Common.getShortStringSize()) {
      this.data = data;
      return true;
    } else {
      this.data = "";
    }
    return false;
  }

  /**
   * Get value.
   */
  public String getValue() {
    return data;
  }

  @Override
  public String toString() {
    return data;
  }
}
