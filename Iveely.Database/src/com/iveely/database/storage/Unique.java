package com.iveely.database.storage;

import java.io.Serializable;
import java.util.HashSet;

/**
 * Unique for a column.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-26 21:00:21
 */
public class Unique implements Serializable {

    /**
     * Dataset.
     */
    private final HashSet<Object> set;

    /**
     * Constructor.
     */
    public Unique() {
        set = new HashSet<>();
    }

    /**
     * Should insert.
     *
     * @param obj
     * @return
     */
    public boolean shouldInsert(Object obj) {
        Integer code = obj.hashCode();
        if (set.contains(code)) {
            return false;
        }
        set.add(code);
        return true;
    }

    /**
     * Need sync to disk.
     *
     * @return
     */
    public boolean needSync() {
        return false;
    }
}
