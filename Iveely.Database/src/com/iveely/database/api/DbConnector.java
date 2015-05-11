package com.iveely.database.api;

import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.text.json.JsonArray;
import com.iveely.framework.text.json.JsonObject;
import com.iveely.framework.text.json.JsonValue;
import com.iveely.database.api.template.Commander;
import com.iveely.database.api.template.ExchangeCode;
import com.iveely.database.api.template.JsonCountTable;
import com.iveely.database.api.template.JsonDropTable;
import com.iveely.database.api.template.JsonInsertMany;
import com.iveely.database.api.template.JsonInsertOne;
import com.iveely.database.api.template.JsonSelectMany;
import com.iveely.database.api.template.JsonSelectOne;
import com.iveely.database.api.template.JsonTable;
import com.iveely.database.common.Message;
import com.iveely.database.storage.Types;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-11 8:28:42
 */
public class DbConnector {

    /**
     * Connection clien.
     */
    private DbClient client;

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
        this.client = new DbClient(server, port);
        this.tables = new TreeMap<>();
    }

//    public boolean open() {
//        return this.client.open();
//    }
//
    public void close() {
        InternetPacket packet = new InternetPacket();
        packet.setMimeType(0);
        packet.setExecutType(Commander.CLOSE.ordinal());
        packet.setData(Message.getBytes(dbName));
        this.client.send(packet);
    }

    /**
     * Create table.
     *
     * @param tableName
     * @return
     */
    public boolean createTable(String tableName, String[] columns, Types[] columnTypes, boolean[] uniques) {
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
            InternetPacket packet = new InternetPacket();
            packet.setMimeType(0);
            packet.setExecutType(Commander.CREATETABLE.ordinal());
            packet.setData(Message.getBytes(tableJson));
            InternetPacket resPacket = this.client.send(packet);
            if (resPacket.getExecutType() == ExchangeCode.TABLE_CREATE_SUCCESS.ordinal()) {
                return true;
            } else {
                return false;
            }
        }
    }

    /**
     * Insert data to database.
     *
     * @param tableName
     * @param objs
     * @return
     */
    public int insert(String tableName, Object[] objs) {
        if (!tables.containsKey(tableName) || objs == null) {
            return -1;
        } else {
            if ((Integer) tables.get(tableName) != objs.length) {
                return -1;
            }
            JsonInsertOne table = new JsonInsertOne(dbName, tableName);
            table.setValues(objs);
            String tableJson = table.toJson();
            InternetPacket packet = new InternetPacket();
            packet.setMimeType(0);
            packet.setExecutType(Commander.INSERTONE.ordinal());
            packet.setData(Message.getBytes(tableJson));
            InternetPacket resPacket = this.client.send(packet);
            return resPacket.getExecutType();
        }
    }

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
            InternetPacket packet = new InternetPacket();
            packet.setMimeType(0);
            packet.setExecutType(Commander.INSERTMANY.ordinal());
            packet.setData(Message.getBytes(tableJson));
            InternetPacket resPacket = this.client.send(packet);
            if (resPacket.getExecutType() == ExchangeCode.INSERT_MANY_SUCCESS.ordinal()) {
                String jsonData = Message.getString(resPacket.getData());
                JsonArray array = JsonObject.readToValue(jsonData).asArray();
                Integer[] objs = new Integer[array.size()];
                for (int i = 0; i < array.values().size(); i++) {
                    objs[i] = Integer.parseInt(replace(array.values().get(i).toString()));
                }
                return objs;
            }
            return null;
        }
    }

    public int delete(String tableName, int id) {
        return -1;
    }

    public boolean dropTable(String tableName) {
        JsonDropTable table = new JsonDropTable();
        table.setDbName(dbName);
        table.setTableName(tableName);
        InternetPacket packet = new InternetPacket();
        packet.setMimeType(0);
        packet.setExecutType(Commander.DROPTABLE.ordinal());
        packet.setData(Message.getBytes(table.toJson()));
        InternetPacket resPacket = this.client.send(packet);
        if (resPacket.getExecutType() == ExchangeCode.DROP_TABLE_SUCCESS.ordinal()) {
            return true;
        }
        return false;
    }

    public Integer getCount(String tableName) {
        JsonCountTable table = new JsonCountTable();
        table.setDbName(dbName);
        table.setTableName(tableName);
        InternetPacket packet = new InternetPacket();
        packet.setMimeType(0);
        packet.setExecutType(Commander.COUNT.ordinal());
        packet.setData(Message.getBytes(table.toJson()));
        InternetPacket resPacket = this.client.send(packet);
        if (resPacket.getExecutType() == ExchangeCode.COUNT_TABLE_SUCCESS.ordinal()) {
            Integer count = Integer.parseInt(Message.getString(resPacket.getData()));
            return count;
        }
        return -1;
    }

    public Object[] selectOne(String tableName, int id) {
        if (!tables.containsKey(tableName) || id < 0) {
            return null;
        } else {
            JsonSelectOne table = new JsonSelectOne(dbName, tableName);
            table.setId(id);
            String tableJson = table.toJson();
            InternetPacket packet = new InternetPacket();
            packet.setMimeType(0);
            packet.setExecutType(Commander.SELECTONE.ordinal());
            packet.setData(Message.getBytes(tableJson));
            InternetPacket resPacket = this.client.send(packet);
            if (resPacket.getExecutType() == ExchangeCode.SELECT_ONE_SUCCESS.ordinal()) {
                String jsonData = Message.getString(resPacket.getData());
                JsonArray array = JsonObject.readToValue(jsonData).asArray().values().get(0).asArray();
                Object[] objs = new Object[array.size()];
                for (int i = 0; i < array.values().size(); i++) {
                    objs[i] = replace(array.values().get(i).toString());
                }
                return objs;
            }
            return null;
        }
    }

    public List<Object[]> selectMany(String tableName, int startIndex, int count) {
        if (!tables.containsKey(tableName) || startIndex < 0 || count < 1) {
            return null;
        } else {
            JsonSelectMany table = new JsonSelectMany(dbName, tableName);
            table.setStartIndex(startIndex);
            table.setCount(count);
            String tableJson = table.toJson();
            InternetPacket packet = new InternetPacket();
            packet.setMimeType(0);
            packet.setExecutType(Commander.SELECTMANY.ordinal());
            packet.setData(Message.getBytes(tableJson));
            InternetPacket resPacket = this.client.send(packet);
            if (resPacket.getExecutType() == ExchangeCode.SELECT_MANY_SUCCESS.ordinal()) {
                String jsonData = Message.getString(resPacket.getData());
                try {
                    List<JsonValue> result = JsonObject.readToValue(jsonData.replaceAll("\n", "")).asArray().values();
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

    private String replace(String text) {
        text = text.substring(1, text.length() - 1);
        return text;
    }

    @Override
    public void finalize() {
        //client.close();
        try {
            super.finalize();
        } catch (Throwable ex) {
            Logger.getLogger(DbConnector.class.getName()).log(Level.SEVERE, null, ex);
        }
    }
}
