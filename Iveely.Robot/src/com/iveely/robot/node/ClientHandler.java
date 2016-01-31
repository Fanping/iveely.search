/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.node;

/**
 * @author {Iveely Liu}
 *
 */
public class ClientHandler implements com.iveely.robot.net.Client.IHandler {

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.net.IClient.IHandler#receive(java.lang.String)
	 */
	@Override
	public void receive(String info) {
		System.out.println("[client receive]" + info);
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.net.IClient.IHandler#caught(java.lang.String)
	 */
	@Override
	public void caught(String exception) {
		System.out.println("[Client error]:" + exception);

	}

}