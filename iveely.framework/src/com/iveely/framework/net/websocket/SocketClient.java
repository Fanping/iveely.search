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
package com.iveely.framework.net.websocket;

import org.eclipse.jetty.websocket.api.Session;
import org.eclipse.jetty.websocket.api.StatusCode;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketClose;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketConnect;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketMessage;
import org.eclipse.jetty.websocket.api.annotations.WebSocket;

import java.util.concurrent.CountDownLatch;
import java.util.concurrent.Future;
import java.util.concurrent.TimeUnit;

/**
 * Basic Echo Client Socket
 */
@WebSocket(maxTextMessageSize = 64 * 1024)
public class SocketClient {

  private final CountDownLatch closeLatch;

  @SuppressWarnings("unused")
  private Session session;

  private SocketServer.IHandler processor;

  public SocketClient(SocketServer.IHandler processor) {
    this.closeLatch = new CountDownLatch(1);
    this.processor = processor;
  }

  public boolean awaitClose(int duration, TimeUnit unit) throws InterruptedException {
    return this.closeLatch.await(duration, unit);
  }

  @OnWebSocketClose
  public void onClose(int statusCode, String reason) {
    System.out.printf("Connection closed: %d - %s%n", statusCode, reason);
    this.session = null;
    this.closeLatch.countDown();
  }

  @OnWebSocketConnect
  public void onConnect(Session session) {
    System.out.printf("Got connect: %s%n", session);
    this.session = session;
    try {
      Future<Void> fut;
      fut = session.getRemote().sendStringByFuture("Hello");
      fut.get(2, TimeUnit.SECONDS);
      fut = session.getRemote().sendStringByFuture("Thanks for the conversation.");
      fut.get(2, TimeUnit.SECONDS);
      session.close(StatusCode.NORMAL, "I'm done");
    } catch (Throwable t) {
      t.printStackTrace();
    }
  }

  @OnWebSocketMessage
  public void onMessage(String msg) {
    if (session != null) {
      this.processor.invoke(session.hashCode(), null, msg);
    }
  }
}
