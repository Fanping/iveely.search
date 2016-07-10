/**
 * date   : 2016��1��27�� author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import com.iveely.brain.environment.Variable;
import com.iveely.brain.mind.Brain;
import com.iveely.framework.file.Directory;
import com.iveely.framework.file.FileOperate;

import org.apache.log4j.Logger;

import java.io.File;
import java.util.List;

/**
 * @author {Iveely Liu} Load all set information.
 */
public class SetLoader {

  /**
   * Single instance.
   */
  private static SetLoader loader;

  /**
   * Logger.
   */
  private static Logger logger;

  private SetLoader() {
    logger = Logger.getLogger(SetLoader.class);
  }

  /**
   * Get instance of set loader.
   *
   * @return instance of set loader.
   */
  public static SetLoader getInstance() {
    if (loader == null) {
      synchronized (SetLoader.class) {
        loader = new SetLoader();
      }
    }
    return loader;
  }

  /**
   * Load set from files.
   */
  public void load() {
    // 1. Get path and check.
    String path = Variable.getPathOfSet();
    boolean exist = Directory.isExist(path);
    if (!exist) {
      logger.error(String.format("Path '%s' is not exist.", path));
      return;
    }

    // 2. Get files.
    File[] files = Directory.getFiles(path);
    if (files == null) {
      logger.warn(String.format("Path '%s' is exist, but not found the sub-files.", path));
      return;
    }

    // 3. Read all.
    for (File file : files) {
      List<String> lines = FileOperate.readAllLines(file);
      if (lines.size() > 0) {
        String name = file.getName();
        Brain.getInstance().addSet(name.substring(0, name.lastIndexOf(".")), lines);
      }
    }
  }

}
