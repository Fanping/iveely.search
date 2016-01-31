/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.net;

import java.net.InetSocketAddress;

import org.apache.mina.core.filterchain.DefaultIoFilterChainBuilder;
import org.apache.mina.core.service.IoHandlerAdapter;
import org.apache.mina.core.session.IdleStatus;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolCodecFilter;
import org.apache.mina.filter.codec.textline.TextLineCodecFactory;
import org.apache.mina.transport.socket.SocketAcceptor;
import org.apache.mina.transport.socket.nio.NioSocketAcceptor;

/**
 * @author {Iveely Liu}
 *
 */
public class AsynServer {

	public interface IHandler {

		/**
		 * Process client information.
		 * 
		 * @param info
		 *            The received message.
		 * @return process result.
		 */
		public Packet process(Packet packet);

		/**
		 * Exception caught.
		 * 
		 * @param exception
		 *            The exception information.
		 */
		public void caught(String exception);
	}

	/**
	 * Inner handle information.
	 * 
	 * @author {Iveely Liu}
	 *
	 */
	protected class InnerHandler extends IoHandlerAdapter {
		@Override
		public void messageReceived(IoSession session, Object message) throws Exception {
			super.messageReceived(session, message);
			Packet packet = handler.process(new Packet().toPacket((byte[]) message));
			session.write(packet);
		}

		@Override
		public void exceptionCaught(IoSession session, Throwable cause) throws Exception {
			if (session.isConnected()) {
				session.close();
			}
			System.out.println("-----------");
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
	 * 
	 * @return
	 */
	public boolean open() {
		try {
			if (acceptor == null) {
				acceptor = new NioSocketAcceptor(Runtime.getRuntime().availableProcessors() + 1);
				DefaultIoFilterChainBuilder chain = acceptor.getFilterChain();
				chain.addLast("codec", new ProtocolCodecFilter(new TextLineCodecFactory()));
				acceptor.setHandler(new InnerHandler());
				acceptor.bind(new InetSocketAddress(this.port));
				return true;
			}
			return false;
		} catch (Exception e) {
			e.printStackTrace();
			return false;
		}

	}

	/**
	 * Close service.
	 * 
	 * @return
	 */
	public boolean close() {
		if (acceptor != null) {
			acceptor.dispose();
			return true;
		}
		return false;
	}

}
