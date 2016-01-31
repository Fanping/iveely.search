/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.mind;

import org.objectweb.asm.Handle;

import com.iveely.robot.environment.Variable;
import com.iveely.robot.net.websocket.SocketServer;
import com.iveely.robot.net.websocket.WSHandler;

/**
 * @author {Iveely Liu}
 *
 */
public class Nerve {

	public class EventHandler implements SocketServer.IHandler {

		/**
		 * Callback handler.
		 */
		private WSHandler handler;

		/*
		 * (non-Javadoc)
		 * 
		 * @see
		 * com.iveely.robot.net.websocket.SocketServer.IHandler#invoke(java.lang
		 * .Integer, com.iveely.robot.net.websocket.WSHandler, java.lang.String)
		 */
		@Override
		public String invoke(Integer sessionId, WSHandler handler, String data) {
			System.out.println(data);
			this.handler = handler;
			Brain.getInstance().request(sessionId, this, data);
			return null;
		}

		/*
		 * (non-Javadoc)
		 * 
		 * @see
		 * com.iveely.robot.net.websocket.SocketServer.IHandler#close(java.lang.
		 * Integer)
		 */
		@Override
		public void close(Integer sessionId) {
			// TODO Auto-generated method stub
			Brain.getInstance().release(sessionId);

		}

		/**
		 * Response information back.
		 * 
		 * @param anwser
		 */
		public void response(String anwser) {
			if (handler != null) {
				handler.send(anwser);
			}
		}

	}

	/**
	 * Activation foreign service.
	 * 
	 * @return false means termination of service.
	 */
	public boolean active() {
		try {
			SocketServer server = new SocketServer(new EventHandler(), Variable.getServiceOfPort());
			server.start();
		} catch (Exception e) {
			e.printStackTrace();
		}
		return false;

	}
}
