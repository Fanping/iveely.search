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
package com.iveely.framework.net;

import java.io.IOException;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.UnknownHostException;

/**
 * Internet
 *
 * @author liufanping (liufanping@iveely.com)
 */
public class Internet {

  /**
   * Get avaliable port on local machine.
   */
  public static int getAvaliablePort(int start) {
    for (int i = start; i < 65536; i++) {
      try {
        ServerSocket socket = new ServerSocket(i);
        socket.close();
        return i;
      } catch (IOException exception) {

      }
    }
    return -1;
  }

  /**
   * Get ip address of local machine.
   */
  public static String getLocalIpAddress() {
    try {
      InetAddress addr = InetAddress.getLocalHost();
      String ip = addr.getHostAddress();
      return ip;
    } catch (UnknownHostException ex) {
      return null;
    }
  }
}
