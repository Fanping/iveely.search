/**
 * date   : 2016年1月27日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.util;

import java.io.File;

/**
 * @author {Iveely Liu} Operation of directory.
 */
public final class Directory {

	/**
	 * Check the directory path is exist.
	 * 
	 * @param path
	 * @return directory is exist,true is exist and false is not.
	 */
	public static boolean isExist(String path) {
		if (path == null) {
			return false;
		}
		File file = new File(path);
		return file.isDirectory() && file.exists();
	}

	/**
	 * Get all sub-files. Before call this, should call isExist(path).
	 * 
	 * @param path
	 *            The directory of the path.
	 * @return All sub-files.
	 */
	public static File[] getFiles(String path) {
		File file = new File(path);
		return file.listFiles();
	}

}
