/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.daiml;

import java.util.List;

import org.dom4j.Element;

import com.iveely.robot.mind.React.Status;

/**
 * @author {Iveely Liu}
 *
 */
public class RequestTemplate implements ITemplate{

	/* (non-Javadoc)
	 * @see com.iveely.robot.daiml.ITemplate#parse(org.dom4j.Element)
	 */
	@Override
	public boolean parse(Element element) {
		// TODO Auto-generated method stub
		return false;
	}

	/* (non-Javadoc)
	 * @see com.iveely.robot.daiml.ITemplate#getType()
	 */
	@Override
	public Status getStatus() {
		// TODO Auto-generated method stub
		return null;
	}

	/* (non-Javadoc)
	 * @see com.iveely.robot.daiml.ITemplate#getResult()
	 */
	@Override
	public String getResult(List<String> stars) {
		// TODO Auto-generated method stub
		return null;
	}

}
