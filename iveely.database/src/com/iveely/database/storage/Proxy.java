package com.iveely.database.storage;

import java.io.Serializable;

/**
 * Proxy object to store.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-27 10:49:45
 */
public class Proxy implements Serializable{

    /**
     * Store id.
     */
    private Integer id;

    /**
     * The objects store to disk.
     */
    private Object[] objs;

    /**
     * Constructor.
     */
    public Proxy() {
        id = -1;
    }

    /**
     * Set object.
     *
     * @param objs
     */
    public void setObjects(Object[] objs) {
        this.objs = objs;
    }

    public Object[] getObjects() {
        return this.objs;
    }

    /**
     * @return the id
     */
    public Integer getId() {
        return id;
    }

    /**
     * @param id the id to set
     */
    public void setId(Integer id) {
        this.id = id;
    }

}
