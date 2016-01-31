/**
 * date   : 2016年1月31日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.daiml;

import org.dom4j.Element;

import com.iveely.robot.mind.React.Status;

/**
 * @author {Iveely Liu}
 *
 */
public class Ret {

	/**
	 * Status of the ret.
	 */
	private Status status;

	/**
	 * The expression of the ret.
	 */
	private String express;

	/**
	 * @return the status
	 */
	public Status getStatus() {
		return status;
	}

	/**
	 * @return the express
	 */
	public String getExpress() {
		return express;
	}

	/**
	 * @param express
	 *            the express to set
	 */
	public void setExpress(String express) {
		this.express = express;
	}

	/**
	 * Parse ret element.
	 * 
	 * @param element
	 * @return true is successfully parse.
	 */
	public boolean parse(Element element) {
		if (element == null) {
			return false;
		}
		express = element.asXML().replace("<ret>", "").replace("</ret>", "").trim();
		if (express.startsWith("<srai>") && express.endsWith("</srai>")) {
			express = express.replace("<srai>", "").replace("</srai>", "");
			this.status = Status.RECURSIVE;
		} else {
			this.status = Status.SUCCESS;
		}
		return true;
	}
}
