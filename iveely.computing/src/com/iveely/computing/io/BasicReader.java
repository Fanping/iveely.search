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

import org.apache.log4j.Logger;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStreamReader;

/**
 * Basic file stream to read,Is the basic file read by the operating system.
 */
public class BasicReader implements IReader {

  private final Logger logger = Logger.getLogger(BasicReader.class);

  /**
   * Can read data.
   */
  private boolean hasNext;

  /**
   * The buffer reader for file.
   */
  private BufferedReader reader;

  /**
   * The data read by line.
   */
  private String line;

  /**
   * Build basic reader,
   */
  public BasicReader() {
    this.hasNext = false;
    this.line = null;
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.computing2.io.IReader#onStart()
   */
  @Override
  public boolean onOpen(String filePath) {
    this.hasNext = false;
    this.line = null;
    File file = new File(filePath);
    if (!file.exists() || file.isDirectory()) {
      return false;
    }
    try {
      InputStreamReader isr = new InputStreamReader(new FileInputStream(file), "UTF-8");
      this.reader = new BufferedReader(isr);
      this.line = reader.readLine();
      this.hasNext = true;
      return true;
    } catch (Exception e) {
      logger.error("When open file on basic reader,exception happend.", e);
    }
    return false;
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.computing2.io.IReader#onRead(java.io.File)
   */
  @Override
  public String onRead() {
    try {
      String text = this.line;
      this.line = reader.readLine();
      return text;
    } catch (Exception e) {
      logger.error("When read file on basic reader,exception happend.", e);
    }
    hasNext = false;
    return null;
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.computing2.io.IReader#onFinish()
   */
  @Override
  public boolean onClose() {
    hasNext = false;
    try {
      if (reader != null) {
        reader.close();
      }
      return true;
    } catch (Exception e) {
      logger.error("When close file on basic reader,exception happend.", e);
    }
    return false;
  }

  @Override
  public boolean hasNext() {
    return this.hasNext && this.line != null;
  }
}
