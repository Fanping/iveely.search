/**
 * date   : 2016年1月31日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import java.util.ArrayList;
import java.util.List;

import org.dom4j.Element;

import com.iveely.brain.mind.React.Status;

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
	 * Index id of the star.
	 */
	private List<Integer> sIds;

	/**
	 * Request id of the node.
	 */
	private List<Integer> rIds;

	public Ret() {
		this.sIds = new ArrayList<>();
		this.rIds = new ArrayList<>();
		this.status = Status.SUCCESS;
	}

	/**
	 * @return the status
	 */
	public Status getStatus() {
		return status;
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
		List<Element> children = element.elements();
		// 1.Just srai
		if (children.size() == 1) {
			String tag = children.get(0).getName();
			if (tag.equals("srai")) {
				this.status = status.RECURSIVE;
				children = children.get(0).elements();
			}
		}

		// 2. Traversed to find replacement identification(star\node)
		for (Element child : children) {
			String tag = child.getName();
			if (tag.equals("node")) {
				int id = Integer.parseInt(child.attributeValue("index"));
				this.rIds.add(id);
				child.setText("%n" + id + "%");
			} else if (tag.equals("star")) {
				int id = Integer.parseInt(child.attributeValue("index"));
				this.sIds.add(id);
				child.setText("%s" + id + "%");
			} else {
				return false;
			}
		}
		this.express = element.getStringValue().trim();
		return true;
	}

	/**
	 * Get return content.
	 * 
	 * @param stars
	 * @param nodes
	 * @return
	 */
	public String getContent(List<String> stars, List<String> nodes) {
		String result = express;
		for (Integer id : this.sIds) {
			result = result.replace("%s" + id + "%", stars.get(id - 1));
		}
		for (Integer id : this.rIds) {
			result = result.replace("%n" + id + "%", nodes.get(id - 1));
		}
		return result;
	}
}
