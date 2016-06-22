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

import org.apache.commons.lang3.SerializationUtils;
import org.apache.mina.core.RuntimeIoException;
import org.apache.mina.core.future.ConnectFuture;
import org.apache.mina.core.service.IoHandlerAdapter;
import org.apache.mina.core.session.IdleStatus;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.filter.logging.LogLevel;
import org.apache.mina.filter.logging.LoggingFilter;
import org.apache.mina.transport.socket.SocketConnector;
import org.apache.mina.transport.socket.nio.NioSocketConnector;

import java.net.ConnectException;
import java.net.InetSocketAddress;

/**
 * @author {Iveely Liu}
 */
public class AsynClient {

  /**
   * Default timeout of connection.
   */
  public static final int DEFAULT_CONNECT_TIMEOUT = 50;
  /**
   * Connector to connect server.
   */
  protected SocketConnector connector;
  /**
   * Handler.
   */
  protected IHandler handler;
  /**
   * IP address of the server.
   */
  private String ipAddress;

  /**
   * Service port of server.
   */
  private int port;

  public AsynClient(String ipAddress, int port, IHandler handler) {
    this.ipAddress = ipAddress;
    this.port = port;
    this.handler = handler;
    this.connector = new NioSocketConnector();
    this.connector.setConnectTimeout(DEFAULT_CONNECT_TIMEOUT);
    LoggingFilter loggingFilter = new LoggingFilter();
    loggingFilter.setSessionClosedLogLevel(LogLevel.NONE);
    loggingFilter.setSessionCreatedLogLevel(LogLevel.NONE);
    loggingFilter.setSessionOpenedLogLevel(LogLevel.NONE);
    loggingFilter.setMessageSentLogLevel(LogLevel.NONE);
    loggingFilter.setMessageReceivedLogLevel(LogLevel.NONE);
    this.connector.getFilterChain().addLast("logger", loggingFilter);
    this.connector.getFilterChain().addLast("codec", new ProtocolCodecFilter(
        new PacketCodecFactory()));
    this.connector.setHandler(new InnerHanlder());
  }

  /**
   * Send message to server.
   */
  public boolean send(Packet packet) {
    ConnectFuture future = this.connector.connect(new InetSocketAddress(
        this.ipAddress, this.port));
    try {
      byte[] msg = SerializationUtils.serialize(packet);
      future.awaitUninterruptibly();
      future.getSession().write(msg);
      return true;
    } catch (RuntimeIoException e) {
      e.printStackTrace();
      if (e.getCause() instanceof ConnectException) {
        try {
          if (future.isConnected()) {
            future.getSession().close();
          }
        } catch (RuntimeIoException e1) {
          e1.printStackTrace();
        }
      }
    }
    return false;
  }

  /**
   * Close connection.
   */
  public boolean close() {
    if (!this.connector.isDisposed()) {
      this.connector.dispose();
      return true;
    }
    return false;
  }

  public interface IHandler {

    /**
     * Receive message from server.
     *
     * @param packet The received message.
     */
    public void receive(Packet packet);

    /**
     * Exception caught.
     *
     * @param exception The exception information.
     */
    public void caught(String exception);
  }

  protected class InnerHanlder extends IoHandlerAdapter {

    private void releaseSession(IoSession session) throws Exception {
      if (session.isConnected()) {
        session.close();
      }
    }

    @Override
    public void sessionOpened(IoSession session) throws Exception {

    }

    @Override
    public void sessionClosed(IoSession session) throws Exception {

    }

    @Override
    public void sessionIdle(IoSession session, IdleStatus status)
        throws Exception {
      try {
        releaseSession(session);
      } catch (RuntimeIoException e) {
      }
    }

    @Override
    public void messageReceived(IoSession session, Object message)
        throws Exception {
      super.messageReceived(session, message);
      if (handler != null) {
        byte[] b = (byte[]) message;
        Packet req = (Packet) SerializationUtils.deserialize((byte[]) b);
        handler.receive(req);
      }
      releaseSession(session);
    }

    @Override
    public void exceptionCaught(IoSession session, Throwable cause)
        throws Exception {
      cause.printStackTrace();
      releaseSession(session);
      if (handler != null) {
        handler.caught(cause.toString());
      }
    }

    @Override
    public void messageSent(IoSession session, Object message)
        throws Exception {
      super.messageSent(session, message);
    }
  }
}
