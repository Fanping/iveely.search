/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.mind;

import com.iveely.brain.environment.Variable;
import com.iveely.framework.file.Serialize;

/**
 * @author {Iveely Liu}
 *
 */
public class Self {

	/**
	 * Name of the robot.
	 */
	private String name;

	/**
	 * Dady of the robot.
	 */
	private String dady;

	/**
	 * Birthday of the robot.
	 */
	private String birthday;

	/**
	 * Gender of the robot.
	 */
	private String gender;

	/**
	 * hobby of the robot.
	 */
	private String hobby;

	/**
	 * Single instance;
	 */
	private static Self self;

	private Self() {
		setName("小薇");
		setDady("Iveely Liu");
		setBirthday("2015-05-01");
		setGender("女");
		setHobby("聊天");
	}

	/**
	 * @return the name
	 */
	public String getName() {
		return name;
	}

	/**
	 * @param name
	 *            the name to set
	 */
	public void setName(String name) {
		this.name = name;
	}

	/**
	 * @return the dady
	 */
	public String getDady() {
		return dady;
	}

	/**
	 * @param dady
	 *            the dady to set
	 */
	public void setDady(String dady) {
		this.dady = dady;
	}

	/**
	 * @return the birthday
	 */
	public String getBirthday() {
		return birthday;
	}

	/**
	 * @param birthday
	 *            the birthday to set
	 */
	public void setBirthday(String birthday) {
		this.birthday = birthday;
	}

	/**
	 * @return the gender
	 */
	public String getGender() {
		return gender;
	}

	/**
	 * @param gender
	 *            the gender to set
	 */
	public void setGender(String gender) {
		this.gender = gender;
	}

	/**
	 * @return the hobby
	 */
	public String getHobby() {
		return hobby;
	}

	/**
	 * @param hobby
	 *            the hobby to set
	 */
	public void setHobby(String hobby) {
		this.hobby = hobby;
	}

	/**
	 * Get single instance.
	 * 
	 * @return
	 */
	public static Self getInstance() {
		if (self == null) {
			synchronized (Self.class) {
				if (self == null) {
					if (!loadFromFile()) {
						self = new Self();
						Serialize.toXML(self, Variable.getPathOfSelf());
					}
				}
			}
		}
		return self;
	}

	/**
	 * Load self information from local file.
	 * 
	 * @return
	 */
	private static boolean loadFromFile() {
		self = Serialize.fromXML(Variable.getPathOfSelf());
		return self != null;
	}

}
