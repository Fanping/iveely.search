/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.daiml;

import java.util.ArrayList;
import java.util.List;

import org.dom4j.Element;

import com.iveely.robot.mind.React.Status;

/**
 * @author {Iveely Liu}
 *
 */
public class TSrai extends ITemplate {

	/**
	 * Srai value.
	 */
	private String val;

	/**
	 * Status of srai.
	 */
	private Status status;

	/**
	 * id collection of star.
	 */
	private List<Integer> ids;

	/**
	 * 
	 */
	public TSrai() {
		this.status = Status.RECURSIVE;
		this.ids = new ArrayList<>();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#parse(org.dom4j.Element)
	 */
	@Override
	public boolean parse(Element element) {
		List<Element> children = element.elements();
		for (Element child : children) {
			String tag = child.getName();
			if (tag.equals("star")) {
				int id = Integer.parseInt(child.attributeValue("index"));
				ids.add(id);
				child.setText("%s" + id + "%");
			}
		}
		this.val = element.getStringValue().trim();
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
		return replaceStar(this.val, this.ids, stars);
	}

}
