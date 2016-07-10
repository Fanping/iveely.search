package com.iveely.database.common;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;

/**
 * Configurator for database.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-27 17:23:47
 */
public class Configurator {

  /**
   * Get object from file.
   */
  public static Object load(String filePath) {
    try {
      File file = new File(filePath);
      if (file.exists()) {
        FileInputStream fis = new FileInputStream(filePath);
        ObjectInputStream ois = new ObjectInputStream(fis);
        return ois.readObject();
      }
    } catch (Exception e) {
      e.printStackTrace();
    }
    return null;
  }

  /**
   * Save object to file.
   */
  public static boolean save(String filePath, Object obj) {
    if (obj == null) {
      return false;
    }
    try {
      File file = new File(filePath);
      if (file.exists()) {
        file.delete();
      }
      FileOutputStream output = new FileOutputStream(file);
      ObjectOutputStream objectOutput = new ObjectOutputStream(output);
      objectOutput.writeObject(obj);
      objectOutput.flush();
      objectOutput.close();
      return true;
    } catch (Exception e) {
      e.printStackTrace();
    }
    return false;
  }

}
