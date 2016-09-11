package com.iveely.database.api;

import com.eclipsesource.json.JsonArray;
import com.eclipsesource.json.JsonObject;
import com.eclipsesource.json.JsonValue;
import com.iveely.database.api.template.Commander;
import com.iveely.database.api.template.ExchangeCode;
import com.iveely.database.api.template.JsonCountTable;
import com.iveely.database.api.template.JsonDropTable;
import com.iveely.database.api.template.JsonInsertMany;
import com.iveely.database.api.template.JsonInsertOne;
import com.iveely.database.api.template.JsonSelectMany;
import com.iveely.database.api.template.JsonSelectOne;
import com.iveely.database.api.template.JsonTable;
import com.iveely.database.common.SimpleString;
import com.iveely.database.storage.Types;
import com.iveely.framework.net.Packet;
import com.iveely.framework.net.SyncClient;

import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * @author liufanping@iveely.com
 */
public class DbConnector {

  /**
   * Connection clien.
   */
  private SyncClient client;

  /**
   * Name of the database.
   */
  private String dbName;

  /**
   * All tables.
   */
  private TreeMap<String, Integer> tables;

  public DbConnector(String dbName, String server, int port) {
    this.dbName = dbName;
    this.client = new SyncClient(server, port);
    this.tables = new TreeMap<>();
  }

  // public boolean open() {
  // return this.client.open();
  // }
  //
  public void close() {
    Packet packet = new Packet();
    packet.setMimeType(0);
    packet.setExecuteType(Commander.CLOSE.ordinal());
    packet.setData(new SimpleString(dbName));
    this.client.send(packet);
  }

  /**
   * Create table.
   *
   * @param tableName   table name
   * @param columns     columns
   * @param columnTypes column types
   * @param uniques     uniques
   * @return whether created success
   */
  public boolean createTable(String tableName,
                             String[] columns,
                             Types[] columnTypes,
                             boolean[] uniques) {
    if (tables.containsKey(tableName)) {
      return false;
    } else {
      if (columnTypes.length != columns.length || columns.length != uniques.length) {
        return false;
      }
      tables.put(tableName, columns.length);
      JsonTable table = new JsonTable(dbName);
      table.setTableName(tableName);
      table.setColumns(columns);
      Integer[] types = new Integer[columnTypes.length];
      for (int i = 0; i < columnTypes.length; i++) {
        types[i] = columnTypes[i].ordinal();
      }
      table.setTypes(types);
      table.setUniques(uniques);
      String tableJson = table.toJson();
      Packet packet = new Packet();
      packet.setMimeType(0);
      packet.setExecuteType(Commander.CREATETABLE.ordinal());
      packet.setData(new SimpleString(tableJson));
      Packet resPacket = this.client.send(packet);
      if (resPacket.getExecuteType() == ExchangeCode.TABLE_CREATE_SUCCESS.ordinal()) {
        return true;
      } else {
        return false;
      }
    }
  }

  /**
   * Insert data to database.
   *
   * @param tableName table name
   * @param objects   objects
   * @return last id
   */
  public int insert(String tableName, Object[] objects) {
    if (!tables.containsKey(tableName) || objects == null) {
      return -1;
    } else {
      if ((Integer) tables.get(tableName) != objects.length) {
        return -1;
      }
      JsonInsertOne table = new JsonInsertOne(dbName, tableName);
      table.setValues(objects);
      String tableJson = table.toJson();
      Packet packet = new Packet();
      packet.setMimeType(0);
      packet.setExecuteType(Commander.INSERTONE.ordinal());
      packet.setData(new SimpleString(tableJson));
      Packet resPacket = this.client.send(packet);
      if (resPacket.getExecuteType() == -1) {
        System.out.println(((SimpleString) resPacket.getData()).getValue());
      }
      return resPacket.getExecuteType();
    }
  }

  /**
   * Insert many data.
   *
   * @param tableName table name
   * @param list      list
   * @return ids
   */
  public Integer[] insertMany(String tableName, List<Object[]> list) {
    if (!tables.containsKey(tableName) || list == null || list.size() == 0) {
      return null;
    } else {
      if ((Integer) tables.get(tableName) != list.get(0).length) {
        return null;
      }
      JsonInsertMany table = new JsonInsertMany(dbName, tableName);
      table.setValues(list);
      String tableJson = table.toJson();
      Packet packet = new Packet();
      packet.setMimeType(0);
      packet.setExecuteType(Commander.INSERTMANY.ordinal());
      packet.setData(new SimpleString(tableJson));
      Packet resPacket = this.client.send(packet);
      if (resPacket.getExecuteType() == ExchangeCode.INSERT_MANY_SUCCESS.ordinal()) {
        String jsonData = ((SimpleString) resPacket.getData()).getValue();
        JsonArray array = JsonObject.readFrom(jsonData).asArray();
        Integer[] objs = new Integer[array.size()];
        for (int i = 0; i < array.values().size(); i++) {
          objs[i] = Integer.parseInt(replace(array.values().get(i).toString()));
        }
        return objs;
      }
      return null;
    }
  }

  /**
   * delete a record by id.
   *
   * @param tableName table name
   * @param id        Id
   * @return return is success
   */
  public int delete(String tableName, int id) {
    return -1;
  }

  /**
   * drop table
   * @param tableName table name
   * @return whether drop table success
   */
  public boolean dropTable(String tableName) {
    JsonDropTable table = new JsonDropTable();
    table.setDbName(dbName);
    table.setTableName(tableName);
    Packet packet = new Packet();
    packet.setMimeType(0);
    packet.setExecuteType(Commander.DROPTABLE.ordinal());
    packet.setData(new SimpleString(table.toJson()));
    Packet resPacket = this.client.send(packet);
    if (resPacket.getExecuteType() == ExchangeCode.DROP_TABLE_SUCCESS.ordinal()) {
      return true;
    }
    return false;
  }

  /**
   * get count
   * @param tableName table name
   * @return count
   */
  public Integer getCount(String tableName) {
    JsonCountTable table = new JsonCountTable();
    table.setDbName(dbName);
    table.setTableName(tableName);
    Packet packet = new Packet();
    packet.setMimeType(0);
    packet.setExecuteType(Commander.COUNT.ordinal());
    packet.setData(new SimpleString(table.toJson()));
    Packet resPacket = this.client.send(packet);
    if (resPacket.getExecuteType() == ExchangeCode.COUNT_TABLE_SUCCESS.ordinal()) {
      Integer count = Integer.parseInt(((SimpleString) resPacket.getData()).getValue());
      return count;
    }
    return -1;
  }

  /**
   * get one record
   * @param tableName table name
   * @param id record id
   * @return data
   */
  public Object[] selectOne(String tableName, int id) {
    if (!tables.containsKey(tableName) || id < 0) {
      return null;
    } else {
      JsonSelectOne table = new JsonSelectOne(dbName, tableName);
      table.setId(id);
      String tableJson = table.toJson();
      Packet packet = new Packet();
      packet.setMimeType(0);
      packet.setExecuteType(Commander.SELECTONE.ordinal());
      packet.setData(new SimpleString(tableJson));
      Packet resPacket = this.client.send(packet);
      if (resPacket != null && resPacket.getExecuteType() == ExchangeCode
              .SELECT_ONE_SUCCESS.ordinal()) {
        String jsonData = ((SimpleString) resPacket.getData()).getValue();
        JsonArray array = JsonObject.readFrom(jsonData).asArray().values().get(0).asArray();
        Object[] objs = new Object[array.size()];
        for (int i = 0; i < array.values().size(); i++) {
          objs[i] = replace(array.values().get(i).toString());
        }
        return objs;
      }
      return null;
    }
  }

  /**
   * Select many records
   * @param tableName table name
   * @param startIndex from start index
   * @param count get count
   * @return all data
   */
  public List<Object[]> selectMany(String tableName, int startIndex, int count) {
    if (!tables.containsKey(tableName) || startIndex < 0 || count < 1) {
      return null;
    } else {
      JsonSelectMany table = new JsonSelectMany(dbName, tableName);
      table.setStartIndex(startIndex);
      table.setCount(count);
      String tableJson = table.toJson();
      Packet packet = new Packet();
      packet.setMimeType(0);
      packet.setExecuteType(Commander.SELECTMANY.ordinal());
      packet.setData(new SimpleString(tableJson));
      Packet resPacket = this.client.send(packet);
      if (resPacket.getExecuteType() == ExchangeCode.SELECT_MANY_SUCCESS.ordinal()) {
        String jsonData = ((SimpleString) resPacket.getData()).getValue();
        try {
          List<JsonValue> result = JsonObject.readFrom(jsonData.replaceAll("\n", "")).asArray().values();
          List<Object[]> objs = new ArrayList<>();
          for (JsonValue jv : result) {
            JsonArray array = jv.asArray();
            Object[] obj = new Object[array.size()];
            for (int i = 0; i < array.values().size(); i++) {
              obj[i] = replace(array.values().get(i).toString());
            }
            objs.add(obj);
          }
          return objs;
        } catch (Exception e) {
          e.printStackTrace();
        }
      }
      return null;
    }
  }

  /**
   * replace text
   * @param text text
   * @return text
   */
  private String replace(String text) {
    text = text.substring(1, text.length() - 1);
    return text;
  }

  @Override
  public void finalize() {
    // client.close();
    try {
      super.finalize();
    } catch (Throwable ex) {
      Logger.getLogger(DbConnector.class.getName()).log(Level.SEVERE, null, ex);
    }
  }
}
