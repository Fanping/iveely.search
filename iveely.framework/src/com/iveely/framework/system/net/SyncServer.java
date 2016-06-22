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
package com.iveely.framework.system.net;

import com.iveely.framework.system.text.Convertor;

import org.apache.log4j.Logger;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.ExecutorService;

/**
 * @author {Iveely Liu}
 */
public class SyncServer {

  /**
   * Client count.
   */
  private final int currentClientCount;
  /**
   * Max client count.
   */
  private final int MAX_CLIENT_COUNT = 1024;
  /**
   * Service port.
   */
  private final int port;
  /**
   * Logger
   */
  private final Logger logger = Logger.getLogger(SyncServer.class);
  /**
   * Call back of the message.
   */
  private ICallback callback;
  /**
   * Executor service.
   */
  private ExecutorService executorService;

  public SyncServer(ICallback callback, int port) {
    currentClientCount = 0;
    if (port > 0 && port < 65535) {
      this.port = port;
    } else {
      logger.error(port + " is not in 0~65535");
      this.port = -1;
    }
    if (callback != null) {
      this.callback = callback;
    } else {
      logger.error("Call back function can not be null.");
    }
  }

  /**
   * Start service.
   */
  public void start() throws IOException {

    ServerSocket serverSocket = new ServerSocket(port);
    // logger.info("sss");
    while (true) {
      Socket socket = serverSocket.accept();
      Executor executor = new Executor(socket, callback);
      executor.start();
    }
  }

  /**
   * Get the service of port.
   */
  public int getPort() {
    return this.port;
  }

  /**
   * Message call back.
   *
   * @author sea11510@mail.ustc.edu.cn
   */
  public interface ICallback {

    /**
     * call back method.
     *
     * @param packet InternetPacket
     */
    public Packet invoke(Packet packet);
  }

  /**
   * Server executor.
   *
   * @author sea11510@mail.ustc.edu.cn
   * @date 2014-10-18 11:23:11
   */
  private class Executor extends Thread {

    /**
     * The client socket.
     */
    private final Socket socket;

    /**
     * The message call back.
     */
    private final ICallback callback;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Executor.class);

    public Executor(Socket socket, ICallback callback) {
      this.socket = socket;
      this.callback = callback;
    }

    @Override
    public void run() {
      try {
        InputStream in = this.socket.getInputStream();
        OutputStream out = this.socket.getOutputStream();
        while (true) {
          // 1. Get message.
          byte[] receivBufLength = new byte[4];
          int readySize = in.read(receivBufLength, 0, 4);
          while (readySize < 4) {
            readySize += in.read(receivBufLength, readySize, 4 - readySize);
          }
          int reviceSize = Convertor.bytesToInt(receivBufLength);
          byte[] receivBuf = new byte[reviceSize];
          int readCount = 0;
          while (readCount < reviceSize) {
            readCount += in.read(receivBuf, readCount, reviceSize - readCount);
          }
          Packet packet = new Packet();
          packet = packet.toPacket(receivBuf);
          if (packet == null) {
            break;
          }
          if (packet.getExecuteType() != -1000) {
            // 2. Process message.
            packet = this.callback.invoke(packet);
          }

          // 3. Response message.
          byte[] bytes = packet.toBytes();
          byte[] feedbackSizeBytes = Convertor.int2byte(bytes.length);
          out.write(feedbackSizeBytes);
          out.write(bytes);
          out.flush();
        }
        // this.socket.close();
      } catch (IOException e) {
        logger.error(e);
      }
    }
  }
}
