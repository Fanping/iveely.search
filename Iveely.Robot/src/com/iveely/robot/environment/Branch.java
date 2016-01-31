/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.environment;

import java.util.List;

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
}
