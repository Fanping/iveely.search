/**
 * date   : 2016年1月31日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.daiml;

import org.dom4j.Element;

import com.iveely.robot.mind.Brain;
import com.thoughtworks.xstream.io.binary.Token.Attribute;

/**
 * @author {Iveely Liu}
 *
 */
public class BranchNode {

	/**
	 * Name of the server to send request.
	 */
	private String name;

	/**
	 * The parameter send to the server.
	 */
	private String parameter;

	/**
	 * @return the name
	 */
	public String getName() {
		return name;
	}

	/**
	 * @param name
	 *            the name to set
	 */
	public void setName(String name) {
		this.name = name;
	}

	/**
	 * @return the parameter
	 */
	public String getParameter() {
		return parameter;
	}

	/**
	 * @param parameter
	 *            the parameter to set
	 */
	public void setParameter(String parameter) {
		this.parameter = parameter;
	}

	/**
	 * Parse to request information.
	 * 
	 * @param element
	 * @return
	 */
	public boolean parse(Element element) {
		if (element == null) {
			return false;
		}

		// 1. Get parameter.
		org.dom4j.Attribute paraAtt = element.attribute("parameter");
		if (paraAtt == null) {
			return false;
		}
		setParameter(paraAtt.getValue());
		org.dom4j.Attribute nameAtt = element.attribute("name");
		if (nameAtt == null) {
			return false;
		}

		// 2. Get name of branch,and check it.
		String nav = nameAtt.getValue();
		if (!Brain.getInstance().isBranchExist(nav)) {
			return false;
		}
		setName(nameAtt.getValue());
		return true;
	}
}
