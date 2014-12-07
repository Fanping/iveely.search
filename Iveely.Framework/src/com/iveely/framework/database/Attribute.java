package com.iveely.framework.database;

/**
 * Table's attribute.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 21:57:22
 */
public class Attribute {

    /**
     * Field type.
     */
    private String fieldType;

    /**
     * Set field type.
     *
     * @param type
     */
    public void setFieldType(String type) {
        fieldType = type;
    }

    /**
     * Get field type.
     *
     * @return 字段类型
     */
    public String getFieldType() {
        return fieldType;
    }

    /**
     * Field name.
     */
    private String fieldName;

    /**
     * Set field name.
     *
     * @param name field name.
     */
    public void setFieldName(String name) {
        fieldName = name;
    }

    /**
     * Get field name.
     *
     * @return field name.
     */
    public String getFieldName() {
        return fieldName;
    }

    @Override
    public String toString() {
        return fieldType + " " + fieldName;
    }
}
