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

import com.thoughtworks.xstream.XStream;

import org.apache.log4j.Logger;

import java.io.FileInputStream;
import java.io.FileOutputStream;

/**
 * @author {Iveely Liu}
 */
public class XmlSerialize {

  private static Logger logger = Logger.getLogger(XmlSerialize.class);

  /**
   * Serialize an object to a file.
   *
   * @param t    The object to be serialized.
   * @param path The path of final file.
   * @return true is success,or is not.
   */
  public static <T> boolean toXML(T t, String path) {
    XStream xStream = new XStream();
    xStream.alias(t.getClass().getName(), t.getClass());
    try {
      FileOperate.deleteFile(path);
      FileOutputStream fileOutputStream = new FileOutputStream(path);
      xStream.toXML(t, fileOutputStream);
      fileOutputStream.close();
      return true;
    } catch (Exception e) {
      logger.error(e.getMessage(), e.getCause());
    }
    return false;
  }

  /**
   * Get object instance from xml file.
   *
   * @param path The path of xml.
   * @return instance of the object.
   */
  public static <T> T fromXML(String path) {
    XStream xStream = new XStream();
    FileInputStream fileInputStream = null;
    try {
      fileInputStream = new FileInputStream(path);
      Object obj = xStream.fromXML(fileInputStream);
      if (obj == null) {
        return null;
      } else {
        return (T) obj;
      }
    } catch (Exception e) {
      logger.error(e.getMessage(), e.getCause());
    } finally {
      try {
        if (fileInputStream != null) {
          fileInputStream.close();
        }
      } catch (Exception e) {
        logger.error(e.getMessage(), e.getCause());
      }
    }
    return null;
  }

}
