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

import com.iveely.framework.text.Convertor;

import org.apache.log4j.Logger;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;

/**
 * @author {Iveely Liu}
 */
public class SyncClient {
  /**
   * Server ip address.
   */
  private final String hostAddress;
  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(SyncClient.class);
  /**
   * Server port.
   */
  private int port;
  /**
   * Socket client.
   */
  private Socket client;

  private int MAX_BUFFER = 1048576;

  public SyncClient(String hostAddres, int port) {
    this.hostAddress = hostAddres;
    this.port = port;
    if (port > 0 && port < 65535) {
      this.port = port;
    } else {
      logger.error(port + " is not in 0~65535");
      this.port = -1;
    }
  }

  /**
   * Send synchronous message.
   */
  public Packet send(Packet message) {
    try {

      try {
        if (this.client == null || this.client.isClosed()) {
          this.client = new Socket(this.hostAddress, this.port);
        }
      } catch (IOException ex) {
        logger.error(ex);
        return Packet.getUnknownPacket();
      }
      // 1. Prepare.
      OutputStream outputStream = client.getOutputStream();

      // 2. Data convert.
      byte[] bytes = message.toBytes();
      byte[] lengthBytes = Convertor.int2byte(bytes.length);
      outputStream.write(lengthBytes);
      outputStream.write(bytes);

      // 3. Response message.
      InputStream inputStream = client.getInputStream();
      int readySize = inputStream.read(lengthBytes, 0, 4);
      while (readySize < 4) {
        readySize += inputStream.read(lengthBytes, readySize, 4 - readySize);
      }
      int offset = Convertor.bytesToInt(lengthBytes);
      if (offset < MAX_BUFFER && offset > 0) {
        byte[] feedbackBytes = new byte[offset];
        int readCount = 0;
        while (readCount < offset) {
          readCount += inputStream.read(feedbackBytes, readCount, offset - readCount);
        }
        Packet feedbackPacket = new Packet();
        feedbackPacket = feedbackPacket.toPacket(feedbackBytes);
        return feedbackPacket;
      } else {
        this.client.close();
        return Packet.getUnknownPacket();
      }

    } catch (IOException e) {
      logger.error(e);
    }
    return Packet.getUnknownPacket();
  }

  public void close() {
    if (this.client != null && !this.client.isClosed()) {
      try {
        Packet packet = new Packet();
        packet.setExecuteType(-1000);
        packet.setData(new byte[]{1, 0, 1});
        send(packet);
        this.client.close();
      } catch (IOException ex) {
        logger.error(ex);
      }
    }
  }
}
