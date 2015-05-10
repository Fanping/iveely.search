package com.iveely.computing.api;

import java.util.List;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 19:53:56
 */
public class Tuple {

    /**
     * Data of the fileds.
     */
    private final List<Object> list;

    public Tuple(List<Object> list) {
        this.list = list;
    }

    public int getSize() {
        return this.list.size();
    }

    public Object get(int index) {
        return list.get(index);
    }
}
