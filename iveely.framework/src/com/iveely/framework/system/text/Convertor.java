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
package com.iveely.framework.system.text;

/**
 * Convertor for int and byte[4].
 *
 * @author sea11510@mail.ustc.edu.cn
 */
public class Convertor {

  /**
   * Convert byte[4] to int.
   */
  public static int bytesToInt(byte[] bytes) {
    int integer = (bytes[0] & 0xff)
        | ((bytes[1] << 8) & 0xff00)
        | ((bytes[2] << 24) >>> 8)
        | (bytes[3] << 24);
    return integer;
  }

  /**
   * Convert int to byte[4];
   */
  public static byte[] int2byte(int intValue) {
    byte[] bytes = new byte[4];
    bytes[0] = (byte) (intValue & 0xff);
    bytes[1] = (byte) ((intValue >> 8) & 0xff);
    bytes[2] = (byte) ((intValue >> 16) & 0xff);
    bytes[3] = (byte) (intValue >>> 24);
    return bytes;
  }
}
