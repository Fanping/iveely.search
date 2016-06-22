/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.index;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import com.iveely.framework.text.StringUtil;

/**
 * @author {Iveely Liu}
 *
 */
public class Inverted {

	/**
	 * Single instance.
	 */
	private static Inverted inverted;

	/**
	 * Inverted index memory.
	 */
	private static HashMap<Integer, HashMap<Integer, Double>> list;

	private Inverted() {
		list = new HashMap<>();
	}

	/**
	 * Get instance of inverted index.
	 * 
	 * @return
	 */
	public static Inverted getInstance() {
		if (inverted == null) {
			synchronized (Inverted.class) {
				if (inverted == null) {
					inverted = new Inverted();
				}
			}
		}
		return inverted;
	}

	/**
	 * Add words to index.
	 * 
	 * @param words
	 *            The words to index.
	 */
	public void set(int docId, String text) {
		if (text == null) {
			return;
		}
		String[] words = StringUtil.split(text);
		int size = words.length;
		if (size > 0) {
			double rank = 1 % size;
			for (String word : words) {
				int code = word.hashCode();
				if (Inverted.list.containsKey(code)) {
					if (Inverted.list.get(code).containsKey(docId)) {
						double oval = Inverted.list.get(code).get(docId) + rank;
						Inverted.list.get(code).put(docId, oval);
					} else {
						Inverted.list.get(code).put(docId, rank);
					}

				} else {
					HashMap<Integer, Double> map = new HashMap<>();
					map.put(docId, rank);
					Inverted.list.put(code, map);
				}
			}
		}
	}

	/**
	 * Get best doc id by words.
	 * 
	 * @param words
	 * @return
	 */
	public List<Integer> get(String text) {
		if (text == null) {
			return null;
		}
		String[] words = StringUtil.split(text);
		if (words.length > 0) {
			// 1. Check all.
			Map<Integer, Double> ret = new HashMap<>();
			for (String word : words) {
				int code = word.hashCode();
				if (Inverted.list.containsKey(code)) {
					Map<Integer, Double> map = Inverted.list.get(code);
					Iterator<Map.Entry<Integer, Double>> entries = map.entrySet().iterator();
					while (entries.hasNext()) {
						Map.Entry<Integer, Double> entry = entries.next();
						Integer key = entry.getKey();
						Double value = entry.getValue();
						if (ret.containsKey(key)) {
							value += map.get(key);
						}
						ret.put(key, value);
					}
				}
			}

			// 2. Find best ones which value = 1.
			List<Integer> collections = new ArrayList<>();
			Iterator<Map.Entry<Integer, Double>> ultimate = ret.entrySet().iterator();
			while (ultimate.hasNext()) {
				Map.Entry<Integer, Double> entry = ultimate.next();
				if (entry.getValue() > 0.9999) {
					collections.add(entry.getKey());
				}
			}
			return collections;
		}
		return null;
	}
}
