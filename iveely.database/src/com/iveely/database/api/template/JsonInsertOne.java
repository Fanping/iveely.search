package com.iveely.database.api.template;

/**
 * @author liufanping@iveely.com
 * @date 2015-3-28 12:45:44
 */
public final class JsonInsertOne {

  private String dbName;
  private String tableName;
  private Object[] values;

  public JsonInsertOne(String dbName, String tableName) {
    setDbName(dbName);
    setTableName(tableName);
  }

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

  /**
   * @return the values
   */
  public Object[] getValues() {
    return values;
  }

  /**
   * @param values the values to set
   */
  public void setValues(Object[] values) {
    this.values = values;
  }

  public String toJson() {
    return com.iveely.framework.text.JSONUtil.toString(this);
  }
}
