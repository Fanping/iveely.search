package com.iveely.database.api.template;

import java.util.List;

/**
 * @author liufanping@iveely.com
 */
public final class JsonInsertMany {

  private String dbName;
  private String tableName;
  private List<Object[]> values;

  public JsonInsertMany(String dbName, String tableName) {
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
    return com.iveely.framework.text.JSONUtil.toString(this);
  }
}
