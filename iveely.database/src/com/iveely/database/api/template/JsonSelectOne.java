package com.iveely.database.api.template;

/**
 * @author liufanping@iveely.com
 * @date 2015-3-28 14:30:03
 */
public final class JsonSelectOne {

  private String dbName;
  private String tableName;
  private Integer id;

  public JsonSelectOne(String dbName, String tableName) {
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
