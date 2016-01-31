/**
 * date   : 2016年1月29日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.util;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;

import com.thoughtworks.xstream.XStream;

/**
 * @author {Iveely Liu}
 *
 */
public class Serialize {

	/**
	 * Serialize an object to a file.
	 * 
	 * @param t
	 *            The object to be serialized.
	 * @param path
	 *            The path of final file.
	 * @return true is success,or is not.
	 */
	public static <T> boolean toXML(T t, String path) {
		XStream xStream = new XStream();
		xStream.alias(t.getClass().getName(), t.getClass());
		try {
			FileOperate.deleteFile(path);
			FileOutputStream fileOutputStream = new FileOutputStream(path);
			xStream.toXML(t, fileOutputStream);
			fileOutputStream.close();
			return true;
		} catch (Exception e) {
			e.printStackTrace();
		}
		return false;
	}

	/**
	 * Get object instance from xml file.
	 * 
	 * @param path
	 *            The path of xml.
	 * @return instance of the object.
	 */
	public static <T> T fromXML(String path) {
		XStream xStream = new XStream();
		try {
			FileInputStream fileInputStream = new FileInputStream(path);
			Object obj = xStream.fromXML(fileInputStream);
			if (obj == null) {
				return null;
			} else {
				return (T) obj;
			}
		} catch (Exception e) {
			e.printStackTrace();
		}
		return null;
	}

}
