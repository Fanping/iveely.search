/**
 * date   : 2016��1��27�� author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.environment;

/**
 * @author {Iveely Liu} The variables of all robot environment.
 */
public final class Variable {

  /**
   * The path of the set.
   */
  private static String pathOfSet = "ai/set";

  /**
   * The path of the self information.
   */
  private static String pathOfSelf = "ai/property/self.xml";

  /**
   * The path of the branch.
   */
  private static String pathOfBranch = "ai/property/branches.xml";

  /**
   * The path of the category.
   */
  private static String pathOfCategory = "ai/category";

  /**
   * The service port of robot.
   */
  private static int serviceOfPort = 9001;

  /**
   * Is local service.
   */
  private static boolean isLocal = false;

  /**
   * Set local mode.
   */
  public static void setLocal() {
    isLocal = true;
  }

  /**
   * Get environment mode.
   * @return environment mode
   */
  public static boolean isLocalMode() {
    return isLocal;
  }

  /**
   * Get the path of the set.
   *
   * @return The path of the set.
   */
  public static String getPathOfSet() {
    return pathOfSet;
  }

  /**
   * Get the path of the self information.
   *
   * @return The path of self information.
   */
  public static String getPathOfSelf() {
    return pathOfSelf;
  }

  /**
   * Get the path pf the category.
   *
   * @return The path of category.
   */
  public static String getPathOfCategory() {
    return pathOfCategory;
  }

  /**
   * Get service port of the robot.
   *
   * @return service port.
   */
  public static int getServiceOfPort() {
    return serviceOfPort;
  }

  /**
   * Get path of the branch.
   */
  public static String getPathOfBranch() {
    return pathOfBranch;
  }

}
