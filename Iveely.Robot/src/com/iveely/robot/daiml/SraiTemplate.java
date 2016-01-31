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
public class SraiTemplate extends ITemplate {

	/**
	 * Srai value.
	 */
	private String val;

	/**
	 * Status of srai.
	 */
	private Status status;

	/**
	 * 
	 */
	public SraiTemplate() {
		this.status = Status.RECURSIVE;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#parse(org.dom4j.Element)
	 */
	@Override
	public boolean parse(Element element) {
		this.val = element.asXML().replace("<srai>", "").replace("</srai>", "").trim();
		return true;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#getType()
	 */
	@Override
	public Status getStatus() {
		return this.status;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#getResult()
	 */
	@Override
	public String getResult(List<String> stars) {
		return replaceStar(this.val, stars);
	}

}
