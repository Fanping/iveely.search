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
package com.iveely.framework.system.file;

import org.apache.log4j.Logger;

import java.io.BufferedInputStream;
import java.io.BufferedOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;

/**
 * Folder operator.
 *
 * @author sea11510@mail.ustc.edu.cn
 */
public class Folder {

  private static Logger logger = Logger.getLogger(Folder.class);

  /**
   * Directory copy.
   */
  public static boolean copyDirectiory(String sourceDir, String targetDir) throws IOException {
    File dirFile = new File(targetDir);
    if (!dirFile.exists() || (deleteDirectory(targetDir))) {
      if (dirFile.mkdirs()) {
        File[] file = (new File(sourceDir)).listFiles();
        for (File file1 : file) {
          if (file1.isFile()) {
            File sourceFile = file1;
            File targetFile = new File(new File(targetDir).getAbsolutePath() + File.separator + file1.getName());
            copyFile(sourceFile, targetFile);
          }
          if (file1.isDirectory()) {
            String dir1 = sourceDir + "/" + file1.getName();
            String dir2 = targetDir + "/" + file1.getName();
            copyDirectiory(dir1, dir2);
          }
        }
        return true;
      }
    }
    return false;
  }

  /**
   * Copy file.
   */
  public static void copyFile(File sourceFile, File targetFile) throws IOException {
    BufferedInputStream inBuff = null;
    BufferedOutputStream outBuff = null;
    try {
      inBuff = new BufferedInputStream(new FileInputStream(sourceFile));
      outBuff = new BufferedOutputStream(new FileOutputStream(targetFile));
      byte[] b = new byte[1024 * 5];
      int len;
      while ((len = inBuff.read(b)) != -1) {
        outBuff.write(b, 0, len);
      }
    } catch (IOException e) {
      logger.error("copy failure.", e.getCause());
    } finally {
      if (outBuff != null) {
        outBuff.close();
      }
      if (inBuff != null) {
        inBuff.close();
      }
    }
  }

  /**
   * Delete file.
   *
   * @param sPath file name.
   * @return True is success.
   */
  public static boolean deleteFile(String sPath) {
    boolean flag = false;
    File file = new File(sPath);
    if (file.isFile() && file.exists()) {
      if (!file.delete()) {
        flag = file.delete();
      } else {
        flag = true;
      }
    }
    return flag;
  }

  /**
   * Delete directory and files in directory.
   *
   * @param sPath directory path.
   * @return True is success.
   */
  public static boolean deleteDirectory(String sPath) {
    if (!sPath.endsWith(File.separator)) {
      sPath = sPath + File.separator;
    }
    File dirFile = new File(sPath);
    if (!dirFile.exists() || !dirFile.isDirectory()) {
      return false;
    }
    boolean flag = true;
    File[] files = dirFile.listFiles();
    for (File file : files) {
      if (file.isFile()) {
        flag = deleteFile(file.getAbsolutePath());
        if (!flag) {
          break;
        }
      } else {
        flag = deleteDirectory(file.getAbsolutePath());
        if (!flag) {
          break;
        }
      }
    }
    if (!flag) {
      return false;
    }
    return dirFile.delete();
  }
}
