/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.mind;

/**
 * @author {Iveely Liu}
 *
 */
public class React {

	/**
	 * The status for match pattern.
	 * 
	 * @author {Iveely Liu}
	 *
	 */
	public enum Status {
		// The final result.
		SUCCESS,
		// Can not found the result.
		FAILURE,
		// Continue to find answer.
		RECURSIVE
	}

	/**
	 * Status of current react.
	 */
	private Status status;

	/**
	 * Result data.
	 */
	private String ret;

	/**
	 * Hit pattern.
	 */
	private String that;

	public React(Status status) {
		this.status = status;
	}

	/**
	 * @param status
	 *            the status to get
	 */
	public Status getStatus() {
		return this.status;
	}

	/**
	 * @return the ret
	 */
	public String getRet() {
		return ret;
	}

	/**
	 * @param ret
	 *            the ret to set
	 */
	public void setRet(String ret) {
		this.ret = ret;
	}

	/**
	 * Set hit pattern.
	 * 
	 * @param text
	 */
	public void setThat(String text) {
		this.that = text;
	}

	/**
	 * Get that.
	 * 
	 * @return
	 */
	public String getThat() {
		return this.that;
	}
}
