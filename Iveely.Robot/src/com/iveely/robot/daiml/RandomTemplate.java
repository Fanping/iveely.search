/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.daiml;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

import org.dom4j.Element;

import com.iveely.robot.daiml.ITemplate.Type;
import com.iveely.robot.mind.React.Status;

/**
 * @author {Iveely Liu}
 *
 */
public class RandomTemplate extends ITemplate {

	/**
	 * All possible answers.
	 */
	private List<String> list;

	/**
	 * Type of the template.
	 */
	private Status status;

	public RandomTemplate() {
		this.status = Status.SUCCESS;
		this.list = new ArrayList<>();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#parse(java.lang.String)
	 */
	public boolean parse(Element element) {
		List<Element> children = element.elements();
		if (children.size() == 0) {
			return false;
		} else {
			for (Element child : children) {
				list.add(child.asXML().trim().replace("<li>", "").replace("</li>", ""));
			}
		}
		return true;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#getType()
	 */
	public Status getStatus() {
		return this.status;
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#getResult()
	 */
	public String getResult(List<String> stars) {
		int size = list.size();
		int id = new Random().nextInt(size) % (size + 1);
		return replaceStar(list.get(id), stars);
	}

}
