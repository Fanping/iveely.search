package com.iveely.database.storage;

import com.iveely.database.common.Configurator;
import com.iveely.database.type.Base64Image;
import com.iveely.database.type.ShortString;
import com.iveely.framework.file.Folder;

import java.io.IOException;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Table of data.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-26 20:59:33
 */
public class Table implements Serializable {

  /**
   * Is Initialize.
   */
  private boolean isInit = false;

  /**
   * House root.
   */
  private String root;

  /**
   * Name of database.
   */
  private String dbName;

  /**
   * Name of table.
   */
  private String tableName;

  /**
   * Columns with id.
   */
  private TreeMap<String, Integer> columns;

  /**
   * All columns with types.
   */
  private List<Types> columnTypes;

  /**
   * All columns with uniques.
   */
  private TreeMap<Integer, Unique> uniques;

  /**
   * Disk operator.
   */
  private IOBase disk;

  /**
   * Constructor.
   */
  public Table(String dbName, String tableName) {
    if (!this.isInit) {
      this.dbName = dbName;
      this.tableName = tableName;
      this.columns = new TreeMap<>();
      this.columnTypes = new ArrayList<>();
      this.uniques = new TreeMap<>();
      this.root = "Warehouses";
      this.isInit = true;
    }
  }

  /**
   * Add new column.
   */
  public boolean addColumn(String name, Types type, boolean isUnique) {
    if (isInit) {
      if (columns.containsKey(name)) {
        return true;
      }
      columns.put(name, columns.size());
      columnTypes.add(type);
      if (isUnique) {
        uniques.put(columns.size() - 1, new Unique());
        loadUniques();
      }
      return true;
    }
    return false;
  }

  /**
   * Count records.
   */
  public int count() {
    if (isInit) {
      if (this.disk == null) {
        loadIOBase();
      }
      if (this.disk != null) {
        return this.disk.getCount();
      }
    }
    return -1;
  }

  /**
   * Insert data to disk.
   */
  public int insert(Object[] data) {
    if (isVaildData(data) && isUniqueData(data)) {
      try {
        // 1. Store data.
        Proxy proxy = new Proxy();
        proxy.setObjects(data);
        if (this.disk == null) {
          loadIOBase();
          if (this.disk == null) {
            Types[] tempTypes = new Types[this.columnTypes.size()];
            tempTypes = this.columnTypes.toArray(tempTypes);
            this.disk = new IOBase(this.dbName, this.tableName, proxy, tempTypes);
            saveIOBase();
          }
        }
        return this.disk.write(proxy);
      } catch (IllegalArgumentException e) {
        return -1;
      } catch (IOException ex) {
        Logger.getLogger(Table.class.getName()).log(Level.SEVERE, null, ex);
      }
    }
    return -1;
  }

  /**
   * Bulk insert to disk.
   */
  public int bulkInsert(List<Object[]> dataSet) {
    if (dataSet != null && dataSet.size() > 0) {
      List<Proxy> list = new ArrayList<>();
      dataSet.stream().filter((data) -> (isVaildData(data) && isUniqueData(data))).map((data) -> {
        Proxy proxy = new Proxy();
        proxy.setObjects(data);
        return proxy;
      }).forEach((proxy) -> {
        list.add(proxy);
      });
      if (list.size() > 0) {
        Proxy[] proxies = new Proxy[list.size()];
        proxies = list.toArray(proxies);
        if (this.disk == null) {
          loadIOBase();
          if (this.disk == null) {
            Types[] tempTypes = new Types[this.columnTypes.size()];
            tempTypes = this.columnTypes.toArray(tempTypes);
            this.disk = new IOBase(this.dbName, this.tableName, proxies[0], tempTypes);
            saveIOBase();
          }
        }
        return this.disk.write(proxies);
      }
    }
    return -1;
  }

  /**
   * Update by store id.
   */
  public boolean update(Object[] data, int storeId) {
    if (isVaildData(data) && isUniqueData(data)) {
      Proxy proxy = new Proxy();
      proxy.setObjects(data);
      proxy.setId(storeId);
      if (this.disk == null) {
        loadIOBase();
        if (this.disk == null) {
          Types[] tempTypes = new Types[this.columnTypes.size()];
          tempTypes = this.columnTypes.toArray(tempTypes);
          this.disk = new IOBase(this.dbName, this.tableName, proxy, tempTypes);
          saveIOBase();
        }
      }
      return this.disk.update(storeId, proxy);
    }
    return false;
  }

  /**
   * Select data by store id.
   */
  public Object[] selectById(int storeId) {
    if (this.disk == null) {
      loadIOBase();
    }
    if (this.disk != null) {
      Proxy temp = this.disk.read(storeId);
      if (temp != null) {
        return temp.getObjects();
      }
    }
    return null;
  }

  /**
   * Drop table.
   */
  public boolean drop() {
    boolean isDeleted = Folder.deleteDirectory(this.root + "/" + this.dbName + "/" + this.tableName);
    if (isDeleted) {
      clear();
    }
    return isDeleted;
  }

  /**
   * Set name of database.
   */
  public void setDBName(String dbName) {
    this.dbName = dbName;
    if (this.disk != null) {
      this.disk.setDBName(dbName);
      saveIOBase();
    }
  }

  /**
   * Serialize IOBase to config file.
   */
  public void saveIOBase() {
    String configPath = this.root + "/" + this.dbName + "/" + this.tableName + "/iobase.ipart";
    Configurator.save(configPath, this.disk);
  }

  /**
   * Serialize unique to config file.
   */
  public void saveUniques() {
    String configPath = this.root + "/" + this.dbName + "/" + this.tableName + "/uniques.ipart";
    Configurator.save(configPath, this.uniques);
  }

  /**
   * Deserialize IOBase from config file.
   */
  private void loadIOBase() {
    String configPath = this.root + "/" + this.dbName + "/" + this.tableName + "/iobase.ipart";
    Object obj = Configurator.load(configPath);
    if (obj != null) {
      IOBase base = (IOBase) obj;
      if (base != null
          && (base.getDbName() == null ? this.dbName == null : base.getDbName().equals(this.dbName))) {
        if (this.disk == null) {
          this.disk = base;
        }
      } else {
        buildDefaultIOBase();
      }
    } else {
      buildDefaultIOBase();
    }
  }

  /**
   * Deserialize unique from config file.
   */
  private void loadUniques() {
    String configPath = this.root + "/" + this.dbName + "/" + this.tableName + "/uniques.ipart";
    Object obj = Configurator.load(configPath);
    if (obj != null) {
      TreeMap<Integer, Unique> base = (TreeMap<Integer, Unique>) obj;
      if (base != null) {
        this.uniques = base;
      }
    }
  }

  /**
   * Check data[] are unique.
   */
  private boolean isUniqueData(Object[] data) {
    for (int i = 0; i < data.length; i++) {
      Unique unique = this.uniques.get(i);
      if (unique != null) {
        if (!unique.shouldInsert(data[i])) {
          return false;
        }
        if (unique.needSync()) {
          saveUniques();
        }
      }
    }
    return true;
  }

  /**
   * Check is vaild data.
   */
  private boolean isVaildData(Object[] data) {
    return data != null && data.length > 0 && data.length == this.columns.size();
  }

  /**
   * Build default IOBase.
   */
  private boolean buildDefaultIOBase() {
    Proxy proxy = new Proxy();
    if (this.disk == null) {
      Types[] tempTypes = new Types[this.columnTypes.size()];
      tempTypes = this.columnTypes.toArray(tempTypes);
      Object[] objs = new Object[tempTypes.length];
      for (int i = 0; i < tempTypes.length; i++) {
        if (tempTypes[i] == Types.STRING) {
          objs[i] = "";
        } else if (tempTypes[i] == Types.SHORTSTRING) {
          objs[i] = new ShortString("");
        } else if (tempTypes[i] == Types.LONG) {
          objs[i] = 0;
        } else if (tempTypes[i] == Types.INTEGER) {
          objs[i] = 0;
        } else if (tempTypes[i] == Types.IMAGE) {
          objs[i] = new Base64Image("");
        } else if (tempTypes[i] == Types.FLOAT) {
          objs[i] = 0;
        } else if (tempTypes[i] == Types.DOUBLE) {
          objs[i] = 0;
        } else if (tempTypes[i] == Types.CHAR) {
          objs[i] = ' ';
        } else {
          objs[i] = 0;
        }
      }
      proxy.setObjects(objs);
      this.disk = new IOBase(this.dbName, this.tableName, proxy, tempTypes);
      saveIOBase();
      return true;
    }
    return false;
  }

  /**
   * Clear memory.
   */
  private void clear() {
    this.columnTypes = null;
    this.columns = null;
    this.dbName = null;
    this.disk = null;
    this.uniques = null;
  }
}
