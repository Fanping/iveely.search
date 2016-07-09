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
package com.iveely.framework.text;

import org.codehaus.jackson.map.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.io.IOException;

/**
 * JSON data serialization and deserialization operations.
 *
 * @author liufanping (liufanping@iveely.com)
 */
public class JSONUtil {

  private static Logger logger = LoggerFactory.getLogger(JSONUtil.class);

  /**
   * Convert object into a JSON string.
   *
   * @param <T> The type of object.
   * @param obj Object instance.
   * @return JSON string.
   */
  public static <T> String toString(T obj) {
    try {
      ObjectMapper mapper = new ObjectMapper();
      if (!mapper.canSerialize(obj.getClass())) {
        return null;
      }
      return mapper.writeValueAsString(obj);
    } catch (Exception e) {
      logger.error("Write value as string failed.", e);
      return null;
    }
  }

  /**
   * Convert
   *
   * @return Object of specify.
   */
  public static <T> T fromString(String text,Class<T> clazz)
      throws IOException {
    ObjectMapper mapper = new ObjectMapper();
    return mapper.readValue(text, clazz);
  }

  /**
   * Convert file to Object.
   *
   * @param <T>  Object.
   * @param file The JSON file.
   * @return Object.
   */
  public static <T> T fromFile(File file, Class<T> t) {
    try {
      if (!file.exists()) {
        return null;
      }
      ObjectMapper mapper = new ObjectMapper();
      return mapper.readValue(file, t);
    } catch (Exception e) {
      e.printStackTrace();
    }
    return null;
  }

  /**
   * Convert object into a file with JSON string.
   *
   * @param <T>  The type of object.
   * @param obj  Object instance.
   * @param file The file to write.
   * @return True is write success,or is not.
   */
  public static <T> boolean toFile(T obj, File file) {
    if (file == null) {
      return false;
    }
    try {
      ObjectMapper mapper = new ObjectMapper();
      if (!mapper.canSerialize(obj.getClass())) {
        return false;
      }
      mapper.writeValue(file, obj);
    } catch (Exception e) {
      e.printStackTrace();
      return false;
    }
    return true;
  }
}
