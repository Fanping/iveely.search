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
import org.apache.mina.core.service.IoHandlerAdapter;
import org.apache.mina.core.session.IdleStatus;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.filter.logging.LogLevel;
import org.apache.mina.filter.logging.LoggingFilter;
import org.apache.mina.transport.socket.SocketAcceptor;
import org.apache.mina.transport.socket.nio.NioSocketAcceptor;

import java.io.IOException;
import java.net.InetSocketAddress;

/**
 * @author {Iveely Liu}
 */
public class AsynServer {

  /**
   * Handler for process client information.
   */
  protected IHandler handler;
  /**
   * Port number of the server to provide services.
   */
  protected int port;
  /**
   * Socket acceptor.
   */
  protected SocketAcceptor acceptor;

  public AsynServer(int port, IHandler handler) {
    this.port = port;
    this.handler = handler;
  }

  /**
   * Open service.
   */
  public boolean open() throws IOException {
    if (acceptor == null) {
      acceptor = new NioSocketAcceptor(Runtime.getRuntime().availableProcessors() + 1);
      LoggingFilter loggingFilter = new LoggingFilter();
      loggingFilter.setSessionClosedLogLevel(LogLevel.NONE);
      loggingFilter.setSessionCreatedLogLevel(LogLevel.NONE);
      loggingFilter.setSessionOpenedLogLevel(LogLevel.NONE);
      loggingFilter.setMessageSentLogLevel(LogLevel.NONE);
      loggingFilter.setMessageReceivedLogLevel(LogLevel.NONE);
      acceptor.getFilterChain().addLast("logger", loggingFilter);
      acceptor.getFilterChain().addLast("codec", new ProtocolCodecFilter(new PacketCodecFactory()));
      acceptor.setHandler(new InnerHandler());
      acceptor.bind(new InetSocketAddress(this.port));
      return true;
    }
    return false;
  }

  /**
   * Close service.
   */
  public boolean close() {
    if (acceptor != null) {
      acceptor.dispose();
      return true;
    }
    return false;
  }

  public interface IHandler {

    /**
     * Process client information.
     *
     * @param data The received message.
     * @return process result.
     */
    public Packet process(Packet data);

    /**
     * Exception caught.
     *
     * @param exception The exception information.
     */
    public void caught(String exception);
  }

  /**
   * Inner handle information.
   *
   * @author {Iveely Liu}
   */
  protected class InnerHandler extends IoHandlerAdapter {

    @Override
    public void messageReceived(IoSession session, Object message) throws Exception {
      super.messageReceived(session, message);
      byte[] b = (byte[]) message;
      Packet req = (Packet) SerializationUtils.deserialize((byte[]) b);
      Packet resp = handler.process(req);
      if (session.isConnected()) {
        session.write(SerializationUtils.serialize(resp));
      }
    }

    @Override
    public void exceptionCaught(IoSession session, Throwable cause) throws Exception {
      if (session.isConnected()) {
        session.close();
      }
      System.out.println(cause);
      cause.printStackTrace();
      handler.caught(cause.getMessage());
    }

    @Override
    public void messageSent(IoSession session, Object message) throws Exception {
      session.close();
    }

    @Override
    public void sessionClosed(IoSession session) throws Exception {
      super.sessionClosed(session);
    }

    @Override
    public void sessionCreated(IoSession session) throws Exception {
      session.getConfig().setIdleTime(IdleStatus.BOTH_IDLE, 30000);
    }

    @Override
    public void sessionIdle(IoSession session, IdleStatus status) throws Exception {
      session.close();
    }

    @Override
    public void sessionOpened(IoSession session) throws Exception {
      super.sessionOpened(session);
    }
  }

}
