/**
 * date   : 2016年1月27日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.util;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

/**
 * @author {Iveely Liu}
 *
 */
public class FileOperate {

	/**
	 * Read all lines from a file.
	 * 
	 * @param file
	 *            The file to read.
	 * @return All lines with list.
	 */
	public static List<String> readAllLines(File file) {
		BufferedReader reader = null;
		List<String> list = new ArrayList<>();
		try {
			reader = new BufferedReader(new FileReader(file));
			String temp = null;
			while ((temp = reader.readLine()) != null) {
				list.add(temp);
			}
			reader.close();
		} catch (IOException e) {
			e.printStackTrace();
		} finally {
			if (reader != null) {
				try {
					reader.close();
				} catch (IOException e1) {
				}
			}
		}
		return list;
	}

	/**
	 * Delete file by path.
	 * 
	 * @param path
	 *            The path of the file.
	 * @return true is successfully delete,or is not,maybe not exist.
	 */
	public static boolean deleteFile(String path) {
		File file = new File(path);
		if (file.isFile() && file.exists()) {
			return file.delete();
		}
		return false;
	}
}
