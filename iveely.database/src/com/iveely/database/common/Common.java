package com.iveely.database.common;

/**
 * Common for database.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 21:48:49
 */
public class Common {

  /**
   * Get size of shor string.
   *
   * @return size.
   */
  public static int getShortStringSize() {
    return 255;
  }

  /**
   * Get size of each header type.
   *
   * @return size.
   */
  public static int getTypeSize() {
    return 4;
  }

  /**
   * Get default size.
   *
   * @return size.
   */
  public static int getDefaultSize() {
    return 4;
  }

  /**
   * Get size normal string.
   *
   * @return size.
   */
  public static int getLongStringSize() {
    return 4096;
  }

  /**
   * Get base64String size.
   *
   * @return size.
   */
  public static int getBase64StringSize() {
    return 10240 * 5;
  }

  /**
   * Get size of simple String.
   *
   * @return size.
   */
  public static int getSimpleStringSize() {
    return 255;
  }

  /**
   * Get size of boolean.
   *
   * @return size.
   */
  public static int getBooleanSize() {
    return 1;
  }

  /**
   * Get size of int.
   *
   * @return size.
   */
  public static int getIntSize() {
    return 16;
  }

  /**
   * Get size of double.
   *
   * @return size.
   */
  public static int getDoubleSize() {
    return 30;
  }

  /**
   * Get size of char.
   *
   * @return size.
   */
  public static int getCharSize() {
    return 4;
  }
}
