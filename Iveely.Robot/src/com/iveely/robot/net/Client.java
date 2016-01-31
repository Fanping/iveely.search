/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.net;

import java.net.ConnectException;
import java.net.InetSocketAddress;

import org.apache.mina.core.RuntimeIoException;
import org.apache.mina.core.future.ConnectFuture;
import org.apache.mina.core.service.IoHandlerAdapter;
import org.apache.mina.core.session.IdleStatus;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.filter.codec.textline.TextLineCodecFactory;
import org.apache.mina.transport.socket.SocketConnector;
import org.apache.mina.transport.socket.nio.NioSocketConnector;

/**
 * @author {Iveely Liu}
 *
 */
public class Client {

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
		public void sessionIdle(IoSession session, IdleStatus status) throws Exception {
			try {
				releaseSession(session);
			} catch (RuntimeIoException e) {
			}
		}

		@Override
		public void messageReceived(IoSession session, Object message) throws Exception {
			super.messageReceived(session, message);
			handler.receive(message.toString());
			releaseSession(session);
		}

		@Override
		public void exceptionCaught(IoSession session, Throwable cause) throws Exception {
			cause.printStackTrace();
			releaseSession(session);
			handler.caught(cause.toString());
		}

		@Override
		public void messageSent(IoSession session, Object message) throws Exception {
			super.messageSent(session, message);
		}

	}

	public interface IHandler {

		/**
		 * Receive message from server.
		 * 
		 * @param info
		 *            The received message.
		 */
		public void receive(String info);

		/**
		 * Exception caught.
		 * 
		 * @param exception
		 *            The exception information.
		 */
		public void caught(String exception);
	}

	/**
	 * Connector to connect server.
	 */
	protected SocketConnector connector;

	/**
	 * IP address of the server.
	 */
	private String ipAddress;

	/**
	 * Service port of server.
	 */
	private int port;

	/**
	 * Handler.
	 */
	protected IHandler handler;

	/**
	 * Default timeout of connection.
	 */
	public static final int DEFAULT_CONNECT_TIMEOUT = 5;

	public Client(String ipAddress, int port, IHandler handler) {
		this.ipAddress = ipAddress;
		this.port = port;
		this.handler = handler;
		this.connector = new NioSocketConnector();
		this.connector.setConnectTimeout(DEFAULT_CONNECT_TIMEOUT);
		this.connector.getFilterChain().addLast("codec", new ProtocolCodecFilter(new TextLineCodecFactory()));
		this.connector.setHandler(new InnerHanlder());
	}

	/**
	 * Send message to server.
	 * 
	 * @param msg
	 */
	public boolean send(String msg) {
		ConnectFuture future = this.connector.connect(new InetSocketAddress(this.ipAddress, this.port));
		try {
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
	 * 
	 * @return
	 */
	public boolean close() {
		if (!this.connector.isDisposed()) {
			this.connector.dispose();
			return true;
		}
		return false;
	}
}
