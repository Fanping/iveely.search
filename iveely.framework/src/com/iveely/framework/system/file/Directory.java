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

import java.io.File;

/**
 * @author {Iveely Liu} Operation of directory.
 */
public final class Directory {

  /**
   * Check the directory path is exist.
   *
   * @return directory is exist,true is exist and false is not.
   */
  public static boolean isExist(String path) {
    if (path == null) {
      return false;
    }
    File file = new File(path);
    return file.isDirectory() && file.exists();
  }

  /**
   * Get all sub-files. Before call this, should call isExist(path).
   *
   * @param path The directory of the path.
   * @return All sub-files.
   */
  public static File[] getFiles(String path) {
    File file = new File(path);
    return file.listFiles();
  }

}
