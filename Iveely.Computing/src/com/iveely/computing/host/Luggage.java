package com.iveely.computing.host;

import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-7 15:19:24
 */
public class Luggage {

    /**
     * Slaves sorted by performance.
     */
    public final static List<String> performanceSlaves = new ArrayList<>();

    /**
     * All slaves.
     */
    public final static TreeMap<String, Integer> slaves = new TreeMap<>();

}
