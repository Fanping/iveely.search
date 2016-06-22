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

import org.eclipse.jetty.websocket.api.RemoteEndpoint;
import org.eclipse.jetty.websocket.api.Session;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketClose;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketConnect;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketError;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketMessage;
import org.eclipse.jetty.websocket.api.annotations.WebSocket;
import org.eclipse.jetty.websocket.server.WebSocketHandler;
import org.eclipse.jetty.websocket.servlet.WebSocketServletFactory;

/**
 * @author sea11510@mail.ustc.edu.cn
 */
@WebSocket
public class WSHandler extends WebSocketHandler {

  /**
   * Event processor.
   */
  private static SocketServer.IHandler eventProcessor;
  /**
   * Current connector.
   */
  private RemoteEndpoint connector;

  public static void setProcessor(SocketServer.IHandler processor) {
    eventProcessor = processor;
  }

  @OnWebSocketClose
  public void onClose(int statusCode, String reason) {
    eventProcessor.close(connector.hashCode());
  }

  @OnWebSocketError
  public void onError(Throwable t) {
    eventProcessor.close(connector.hashCode());
  }

  @OnWebSocketConnect
  public void onConnect(Session session) {
    connector = session.getRemote();
  }

  @OnWebSocketMessage
  public void onMessage(String message) {
    if (eventProcessor != null) {
      send(eventProcessor.invoke(connector.hashCode(), this, message));
    }
  }

  /**
   * Asynchronous callback.
   *
   * @param msg the information send to the client.
   */
  public void send(String msg) {
    try {
      if (connector != null) {
        connector.sendString(msg);
      }
    } catch (Exception e) {
      e.printStackTrace();
    }

  }

  @Override
  public void configure(WebSocketServletFactory factory) {
    // TODO Auto-generated method stub
    factory.register(WSHandler.class);
  }
}
