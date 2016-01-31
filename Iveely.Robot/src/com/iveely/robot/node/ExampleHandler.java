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
public class ExampleHandler implements com.iveely.robot.net.Server.IHandler {

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.net.IServer.IHandler#process(java.lang.String)
	 */
	@Override
	public String process(String info) {
		return "1";
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.net.IServer.IHandler#caught(java.lang.String)
	 */
	@Override
	public void caught(String exception) {
		System.out.println("[server exception]" + exception);

	}

}
