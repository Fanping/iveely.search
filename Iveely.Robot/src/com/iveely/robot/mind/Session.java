/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.mind;

import java.util.List;

/**
 * @author {Iveely Liu}
 *
 */
public class Session implements Runnable {

	/**
	 * Session result handler.
	 */
	private Nerve.EventHandler handler;

	/**
	 * Question of current session.
	 */
	private String question;

	/**
	 * Last hit pattern.
	 */
	private String that;

	public Session(Nerve.EventHandler handler, String question) {
		this.handler = handler;
		this.question = question;
	}

	/**
	 * @param question
	 *            the question to set
	 */
	public void setQuestion(String question) {
		this.question = question;
	}

	@Override
	public void run() {
		Idio idio = Brain.getInstance().think(this.question, this.that);
		String ret;
		if (idio == null) {
			that = null;
			ret = "Not found.";
		} else {
			that = idio.getThat();
			ret = idio.getResponse();
		}
		handler.response(ret);
	}
}
