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
package com.iveely.computing.io;

/**
 * The basic interface to write data.
 */
public interface IWriter {

  /**
   * According to the file path to open the file.
   *
   * @param filePath The file path to open.
   * @return True is open successed.
   */
  public boolean onOpen(String filePath);

  /**
   * Write one line of text.
   *
   * @param text The text.
   * @return True is write suceessed.
   */
  public boolean onWrite(String text);

  /**
   * Close the written file.
   *
   * @return True is close successed.
   */
  public boolean onClose();
}
