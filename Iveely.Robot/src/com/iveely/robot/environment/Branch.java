/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.environment;

import java.util.List;

import com.iveely.robot.net.Packet;
import com.iveely.robot.net.SyncClient;
import com.iveely.robot.net.SyncServer;
import com.iveely.robot.net.Packet.MimeType;

/**
 * @author {Iveely Liu}
 *
 */
public class Branch {

	/**
	 * IP Address of the branch.
	 */
	private String ipAddress;

	/**
	 * Service port of the branch.
	 */
	private int port;

	/**
	 * Whether branch have enabled
	 */
	private boolean isEnable;

	/**
	 * @return the ipAddress
	 */
	public String getIpAddress() {
		return ipAddress;
	}

	/**
	 * @param ipAddress
	 *            the ipAddress to set
	 */
	public void setIpAddress(String ipAddress) {
		this.ipAddress = ipAddress;
	}

	/**
	 * @return the port
	 */
	public int getPort() {
		return port;
	}

	/**
	 * @param port
	 *            the port to set
	 */
	public void setPort(int port) {
		this.port = port;
	}

	/**
	 * Client to connect the branch server.
	 */
	private SyncClient client;

	/**
	 * Send message to branch.
	 * 
	 * @param msg
	 * @return
	 */
	public Packet send(String msg) {
		if (this.client == null) {
			synchronized (Branch.class) {
				if (this.client == null) {
					this.client = new SyncClient(this.ipAddress, this.port);
				}
			}
		}
		Packet packet = new Packet();
		packet.setMimeType(MimeType.STRING.ordinal());
		packet.setData(msg);
		packet.setExecutType(1);
		return this.client.send(packet);
	}
}
