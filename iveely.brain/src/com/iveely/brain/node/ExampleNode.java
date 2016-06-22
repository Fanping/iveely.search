/**
 * date   : 2016年1月31日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.node;

import com.iveely.framework.net.Packet;
import com.iveely.framework.net.SyncServer;
import com.iveely.framework.net.Packet.MimeType;

/**
 * @author {Iveely Liu}
 *
 */
public class ExampleNode {

	public class Handler implements com.iveely.framework.net.SyncServer.ICallback {

		/*
		 * (non-Javadoc)
		 * 
		 * @see
		 * com.iveely.robot.net.SyncServer.ICallback#invoke(com.iveely.robot.net
		 * .Packet)
		 */
		@Override
		public Packet invoke(Packet packet) {
			packet.setMimeType(MimeType.STRING.ordinal());
			packet.setData("125cm");
			packet.setExecutType(1);
			return packet;
		}
	}

	/**
	 * Synchronous communication.
	 */
	public SyncServer server;

	public ExampleNode(int port) {
		this.server = new SyncServer(new Handler(), port);
	}

	/**
	 * Start example node to help brain more clever.
	 */
	public void start() {
		this.server.start();
	}
}
