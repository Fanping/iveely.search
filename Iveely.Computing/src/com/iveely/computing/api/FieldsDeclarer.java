package com.iveely.computing.api;

import org.apache.log4j.Logger;

/**
 * Field Declarer.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 19:47:16
 */
public class FieldsDeclarer {

    /**
     * All fields.
     */
    private String[] fields;

    /**
     * group list.
     */
    private Integer[] groups;

    /**
     * Logger
     */
    private final Logger logger = Logger.getLogger(FieldsDeclarer.class.getName());

    /**
     * Fields of the tuple.
     *
     * @param fields
     * @param groupBy
     */
    public void declare(String[] fields, Integer[] groupBy) {
        this.fields = fields;
        this.groups = groupBy;
        if (groupBy != null) {
            boolean hasError = false;
            // Make sure the index less than the length.
            for (Integer gb : groupBy) {
                if (fields.length <= gb) {
                    logger.error("When use groupby,please make sure the index less than the length");
                    hasError = true;
                    break;
                }
            }
            if (hasError) {
                this.groups = null;
            }
        }
    }

    /**
     * Get fields.
     *
     * @return
     */
    public String[] getFields() {
        String[] copyVersion = this.fields;
        return copyVersion;
    }

    /**
     * Get groups.
     *
     * @return
     */
    public Integer[] getGroups() {
        Integer[] copyVersion = this.groups;
        return copyVersion;
    }
}
