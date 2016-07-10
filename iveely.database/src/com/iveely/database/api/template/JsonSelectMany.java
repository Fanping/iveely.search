package com.iveely.database.api.template;

/**
 * @author liufanping@iveely.com
 * @date 2015-3-28 15:42:36
 */
public final class JsonSelectMany {

  private String dbName;
  private String tableName;
  private Integer startIndex;
  private Integer count;

  public JsonSelectMany(String dbName, String tableName) {
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

  public String toJson() {
    return com.iveely.framework.text.JSONUtil.toString(this);
  }

  /**
   * @return the startIndex
   */
  public Integer getStartIndex() {
    return startIndex;
  }

  /**
   * @param startIndex the startIndex to set
   */
  public void setStartIndex(Integer startIndex) {
    this.startIndex = startIndex;
  }

  /**
   * @return the count
   */
  public Integer getCount() {
    return count;
  }

  /**
   * @param count the count to set
   */
  public void setCount(Integer count) {
    this.count = count;
  }
}
