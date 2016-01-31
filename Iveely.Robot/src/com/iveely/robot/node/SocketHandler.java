/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.node;

import com.iveely.robot.net.websocket.SocketServer;

/**
 * @author {Iveely Liu}
 *
 */
public class SocketHandler implements SocketServer.IHandler {

	/*
	 * (non-Javadoc)
	 * 
	 * @see
	 * com.iveely.robot.net.websocket.SocketServer.IHandler#invoke(java.lang.
	 * Integer, com.iveely.robot.net.websocket.WSHandler, java.lang.String)
	 */
	@Override
	public String invoke(Integer sessionId, com.iveely.robot.net.websocket.WSHandler handler, String data) {
		System.out.println("Server get:" + data);
		if (handler != null) {
			handler.send("[Hi]" + data);
		}
		return "[Hi2]" + data;
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

	}

}
