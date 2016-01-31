/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.daiml;

import java.security.interfaces.RSAKey;
import java.util.ArrayList;
import java.util.List;

import org.dom4j.Element;

import com.iveely.robot.environment.Script;
import com.iveely.robot.mind.Brain;
import com.iveely.robot.mind.React.Status;
import com.iveely.robot.net.Packet;

import bsh.StringUtil;

/**
 * @author {Iveely Liu}
 *
 */
public class RequestTemplate extends ITemplate {

	/**
	 * Requst information.
	 */
	private List<Request> requests;

	/**
	 * Response detail.
	 */
	private Ret ret;

	/**
	 * Script of the response.
	 */
	private String script;

	public RequestTemplate() {
		this.requests = new ArrayList<>();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#parse(org.dom4j.Element)
	 */
	public boolean parse(Element element) {
		List<Element> children = element.elements();
		if (children.size() == 0) {
			return false;
		}
		for (Element ele : children) {
			String tag = ele.getName().toLowerCase();
			// 1. Parse request.
			if (tag.equals("request")) {
				Request req = new Request();
				if (req.parse(ele)) {
					requests.add(req);
				} else {
					return false;
				}

			}
			// 2. Parse ret.
			else if (tag.equals("ret")) {
				ret = new Ret();
				if (!ret.parse(ele)) {
					return false;
				}
			}
			// 3. Parse script.
			else if (tag.equals("script")) {
				script = ele.getText();
			}
			// 4. Unknown.
			else {
				return false;
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
		return this.ret.getStatus();
	}

	/*
	 * (non-Javadoc)
	 * 
	 * @see com.iveely.robot.daiml.ITemplate#getResult()
	 */
	public String getResult(List<String> stars) {
		// 1. Get parameter of request.
		List<String> nodes = new ArrayList<>();
		for (Request req : requests) {
			String temp = replaceStar(req.getParameter(), stars);
			Packet packet = Brain.getInstance().getBranch(req.getName()).send(temp);
			temp = com.iveely.robot.util.StringUtil.getString(packet.getData());
			nodes.add(temp);
		}

		// 2. Replace ret.
		String retText = ret.getExpress();
		for (int i = nodes.size() - 1; i > -1; i--) {
			retText = retText.replace("%n" + (i + 1) + "%", nodes.get(i));
		}

		// 3. Check is recursion.
		String result = retText;
		if (ret.getStatus() == Status.RECURSIVE) {
			result = Brain.getInstance().think(retText);
		}
		result = replaceStar(result, stars);

		// 4. Execute script.
		if (script.isEmpty()) {
			return result;
		} else {
			String sret = script.replace("%r%", result);
			sret = replaceStar(sret, stars);
			return Script.eval(sret);
		}
	}

}
