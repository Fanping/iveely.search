package com.iveely.framework.text;

import java.util.TreeMap;

/**
 * Hash counter.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-26 23:04:01
 */
public class HashCounter {

    /**
     * Word frequency statistics.
     *
     * @param keywords
     * @return
     */
    public static TreeMap<Integer, Double> statistic(String[] keywords) {
        TreeMap<Integer, Double> counter = new java.util.TreeMap<>();
        if (keywords == null) {
            return counter;
        }
        int totalCount = keywords.length;
        for (String keyword : keywords) {
            int kwHash = keyword.hashCode();
            if (counter.containsKey(kwHash)) {
                double rank = counter.get(kwHash);
                counter.put(kwHash, rank);// + 1.0 / totalCount);
            } else {
                counter.put(kwHash, 1.0);// / totalCount);
            }
        }
        return counter;
    }
}
