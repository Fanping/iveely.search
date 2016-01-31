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
public interface ITemplate {

	/**
	 * All types of the template.
	 * 
	 * @author {Iveely Liu}
	 *
	 */
	public enum Type {
		NORMAL, RANDOM, SRAI, REQUEST, UNKNOW
	}

	/**
	 * Parse the element of template.
	 * 
	 * @param element
	 *            The element of template.
	 * @return true is successfully parse,or is not.
	 */
	public boolean parse(Element element);

	/**
	 * Get status of the template.
	 * 
	 * @return the status of template.
	 */
	public Status getStatus();

	/**
	 * Get result by template.
	 * 
	 * @return the result.
	 */
	public String getResult(List<String> stars);

}
