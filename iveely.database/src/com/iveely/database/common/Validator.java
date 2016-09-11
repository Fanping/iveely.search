package com.iveely.database.common;

/**
 * Validation.
 *
 * @author liufanping@iveely.com
 */
public class Validator {

  /**
   * Check name of table and column.
   */
  public static boolean isLegal(String name) {
    if (name != null) {
      return name.matches("^(?!_)(?!.*?_$)[a-zA-Z0-9_]+$");
    }
    return false;
  }
}
