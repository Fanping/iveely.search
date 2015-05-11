package com.iveely.database.api.template;

import java.util.List;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-28 15:41:26
 */
public final class JsonInsertMany {

    public JsonInsertMany(String dbName, String tableName) {
        setDbName(dbName);
        setTableName(tableName);
    }

    private String dbName;

    /**
     * @return the dbName
     */
    public String getDbName() {
        return dbName;
    }

    /**
     * @param dbName the dbName to set
     */
    public void setDbName(String dbName) {
        this.dbName = dbName;
    }

    private String tableName;

    /**
     * @return the tableName
     */
    public String getTableName() {
        return tableName;
    }

    /**
     * @param tableName the tableName to set
     */
    public void setTableName(String tableName) {
        this.tableName = tableName;
    }

    private List<Object[]> values;

    /**
     * @return the values
     */
    public List<Object[]> getValues() {
        return values;
    }

    /**
     * @param values the values to set
     */
    public void setValues(List<Object[]> values) {
        this.values = values;
    }

    public String toJson() {
        return com.iveely.framework.text.json.JsonUtil.beanToJson(this);
    }
}
