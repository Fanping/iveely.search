/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

/**
 * @author {Iveely Liu}
 *
 */
public class Star {

	/**
	 * Type of the star.
	 * 
	 * @author {Iveely Liu}
	 *
	 */
	enum StarType {
		EVERYTHING, NUMBER, SET, DATETIME, EN_WORD, EMAIL, TELEPHONE, IPADDRESS, URL, UNKNOWN
	}

	/**
	 * Type of the star.
	 */
	private StarType type;

	/**
	 * Value behind of the star.
	 */
	private String value;

	public Star(StarType type) {
		setType(type);
	}

	public Star(StarType type, String val) {
		setType(type);
		setValue(val);
	}

	/**
	 * @return the type
	 */
	public StarType getType() {
		return type;
	}

	/**
	 * @param type
	 *            the type to set
	 */
	public void setType(StarType type) {
		this.type = type;
	}

	/**
	 * @return the value
	 */
	public String getValue() {
		return value;
	}

	/**
	 * @param value
	 *            the value to set
	 */
	public void setValue(String value) {
		this.value = value;
	}

}
