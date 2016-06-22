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
package com.iveely.framework.system.net.websocket;

import org.apache.log4j.Logger;
import org.eclipse.jetty.server.Server;

import java.io.IOException;

/**
 * Websocket.
 *
 * @author sea11510@mail.ustc.edu.cn
 */
public class SocketServer {

  /**
   * logger.
   */
  private final Logger logger = Logger.getLogger(SocketServer.class.getName());
  /**
   * Service port.
   */
  private int port = 8000;

  /**
   * Event processor.
   */
  private IHandler eventProcessor;

  public SocketServer(IHandler processor, int port) throws IOException {
    if (processor == null) {
      throw new NullPointerException();
    }
    this.port = port;
    this.eventProcessor = processor;
    logger.info("Web socket service is init.");
  }

  public void start() {
    try {
      Server server = new Server(this.port);
      WSHandler.setProcessor(eventProcessor);
      server.setHandler(new WSHandler());
      server.setStopTimeout(0);
      server.start();
      server.join();
    } catch (Exception ex) {
      logger.error(ex);
    }
  }

  public interface IHandler {

    /**
     * Call back.
     */
    public String invoke(Integer sessionId, WSHandler handler, String data);

    /**
     * Call back of close event.
     */
    public void close(Integer sessionId);
  }
}
