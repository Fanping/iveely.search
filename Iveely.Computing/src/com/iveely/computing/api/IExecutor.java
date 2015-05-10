package com.iveely.computing.api;

import java.util.HashMap;

/**
 * Executor.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-6 19:43:03
 */
public class IExecutor {

    /**
     * Config.
     */
    protected HashMap<String, Object> _conf;

    /**
     * Output of the collector.
     */
    protected StreamChannel _collector;

    /**
     * Declarer of fields.
     */
    protected FieldsDeclarer _deDeclarer;

    /**
     * Name of the topology.
     */
    protected String _name;

    /**
     * Get name of the toplogy.
     *
     * @return
     */
    public String getName() {
        return this._name;
    }

    /**
     * Get fields.
     *
     * @return
     */
    public String[] getFields() {
        return this._deDeclarer.getFields();
    }

    /**
     * Get groups.
     *
     * @return
     */
    public Integer[] getGroups() {
        return this._deDeclarer.getGroups();
    }

}
