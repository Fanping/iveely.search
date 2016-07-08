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
package com.iveely.framework.file;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;

/**
 * @author {Iveely Liu}
 */
public class FileOperate {

  /**
   * Read all lines from a file.
   *
   * @param file The file to read.
   * @return All lines with list.
   */
  public static List<String> readAllLines(File file) {
    BufferedReader reader = null;
    List<String> list = new ArrayList<>();
    try {
      reader = new BufferedReader(new InputStreamReader(new FileInputStream(file), "utf8"));
      String temp = null;
      while ((temp = reader.readLine()) != null) {
        list.add(temp);
      }
      reader.close();
    } catch (IOException e) {
      e.printStackTrace();
    } finally {
      if (reader != null) {
        try {
          reader.close();
        } catch (IOException e1) {
        }
      }
    }
    return list;
  }

  /**
   * Delete file by path.
   *
   * @param path The path of the file.
   * @return true is successfully delete,or is not,maybe not exist.
   */
  public static boolean deleteFile(String path) {
    File file = new File(path);
    if (file.isFile() && file.exists()) {
      return file.delete();
    }
    return false;
  }
}
