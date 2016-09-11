package com.iveely.database.storage;

import com.iveely.database.LocalStore;
import com.iveely.database.common.Validator;
import com.iveely.framework.file.Folder;

import org.apache.log4j.Logger;

import java.io.File;
import java.io.IOException;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Set;
import java.util.TreeMap;

/**
 * Database instance.
 *
 * @author liufanping@iveely.com
 */
public class Warehouse implements Serializable {

  /**
   * Logger.
   */
  private static final Logger logger = Logger.getLogger(Warehouse.class.getName());

  /**
   * Database name.
   */
  private String dbName;

  /**
   * Last backup name.
   */
  private String lastBakUp;

  /**
   * Root of database.
   */
  private String root;

  /**
   * Table map.
   */
  private TreeMap<String, Table> tables;

  /**
   * Is initialize.
   */
  private boolean isInit = false;

  /**
   * Constructor.
   */
  public Warehouse(String dbName) {
    if (!isInit) {
      this.dbName = dbName;
      this.tables = new TreeMap<>();
      this.root = "Warehouses";
      this.lastBakUp = "";
      isInit = true;
    }
  }

  /**
   * Create table.
   */
  public boolean createTable(String tableName) {
    if (isInit && Validator.isLegal(tableName)) {
      if (tables.containsKey(tableName)) {
        return true;
      }
      Table table = new Table(this.dbName, tableName);
      File file = new File(this.root + "/" + this.dbName + "/" + tableName);
      if (file.exists()) {
        tables.put(tableName, table);
        LocalStore.serialize();
        return true;
      }
      if (file.mkdir()) {
        tables.put(tableName, table);
        LocalStore.serialize();
        return true;
      }
    }
    return false;
  }

  /**
   * Create index on column.
   */
  public boolean createIndex(String tableName, String indexColumn, boolean desc) {
    return false;
  }

  /**
   * Create column.
   */
  public boolean createColumn(String tableName, String columnName, Types type, boolean isUnique) {
    if (isInit && Validator.isLegal(columnName)) {
      Table table = tables.get(tableName);
      if (table != null) {
        return table.addColumn(columnName, type, isUnique);
      }
    }
    return false;
  }

  /**
   * Count records.
   */
  public int count(String tableName) {
    if (isInit) {
      Table table = tables.get(tableName);
      if (table != null) {
        return table.count();
      }
    }
    return -1;
  }

  public String[] getTableNames() {
    Set<String> keys = tables.keySet();
    String[] names = new String[keys.size()];
    names = keys.toArray(names);
    return names;
  }

  /**
   * Insert data.
   */
  public int insert(String tableName, Object[] data) {
    if (isInit) {
      Table table = tables.get(tableName);
      if (table != null) {
        return table.insert(data);
      }
    }
    return -1;
  }

  /**
   * Bulk insert data.
   */
  public int bulkInsert(String tableName, List<Object[]> dataSet) {
    if (isInit) {
      Table table = tables.get(tableName);
      if (table != null) {
        return table.bulkInsert(dataSet);
      }
    }
    return -1;
  }

  /**
   * Update data.
   */
  public boolean update(String tableName, Object[] data, int storeId) {
    if (isInit) {
      Table table = tables.get(tableName);
      if (table != null) {
        return table.update(data, storeId);
      }
    }
    return false;
  }

  /**
   * Select all data.
   */
  public List<Object[]> selectAll(String tableName) {
    int count = selectCount(tableName);
    List<Object[]> list = new ArrayList<>();
    for (int i = 0; i < count; i++) {
      Object[] temp = selectById(tableName, i);
      if (temp != null) {
        list.add(temp);
      } else {
        break;
      }
    }
    return list;
  }

  /**
   * Select data by store id.
   */
  public Object[] selectById(String tableName, int storeId) {
    if (isInit) {
      Table table = tables.get(tableName);
      if (table != null) {
        return table.selectById(storeId);
      }
    }
    return null;
  }

  /**
   * Select data use where.
   */
  public List<Object[]> selectWhere(String tableName, String columnName, Object columnEqual) {
    return null;
  }

  /**
   * Select first record.
   */
  public Object[] selectFirst(String tableName) {
    return null;
  }

  /**
   * Select last record.
   */
  public Object[] selectLast(String tableName) {
    return null;
  }

  /**
   * Select count of table.
   */
  public int selectCount(String tableName) {
    return -1;
  }

  /**
   * Select first record by where condition.
   */
  public Object[] selectFirstOf(String tableName, String columnName, Object columnEqual) {
    return null;
  }

  /**
   * Select last record by where condition.
   */
  public Object[] selectLastOf(String tableName, String columnName, Object columnEqual) {
    return null;
  }

  /**
   * Drop table by name.
   */
  public boolean dropTable(String tableName) {
    if (isInit) {
      Table table = tables.get(tableName);
      if (table != null) {
        tables.remove(tableName);
        boolean ret = table.drop();
        LocalStore.serialize();
        return ret;
      }
    }
    return false;
  }

  /**
   * Rename database.
   */
  public boolean rename(String newName) {
    if (Validator.isLegal(newName)) {
      try {
        if (Folder.copyDirectiory(this.dbName, newName)) {
          this.dbName = newName;
          Iterator<String> iterator = tables.keySet().iterator();
          while (iterator.hasNext()) {
            Table table = tables.get(iterator.next());
            table.setDBName(newName);
          }
        }
      } catch (IOException ex) {
        logger.error(ex);
      }
    }
    return false;
  }

  /**
   * Replace name of database.
   */
  public boolean transferName(String newName) {
    if (Validator.isLegal(newName)) {
      this.dbName = newName;
      Iterator<String> iterator = tables.keySet().iterator();
      while (iterator.hasNext()) {
        Table table = tables.get(iterator.next());
        table.setDBName(newName);
      }
      return true;
    }
    return false;
  }

  public void close() {
    Iterator<String> iterator = tables.keySet().iterator();
    while (iterator.hasNext()) {
      Table table = tables.get(iterator.next());
      table.saveIOBase();
      table.saveUniques();
    }
  }

  /**
   * @return the lastBakUp
   */
  public String getLastBakUp() {
    return lastBakUp;
  }

  /**
   * @param lastBakUp the lastBakUp to set
   */
  public void setLastBakUp(String lastBakUp) {
    this.lastBakUp = lastBakUp;
  }
}
