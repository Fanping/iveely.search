package com.iveely.framework.database;

import com.iveely.framework.file.Folder;
import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * Store engine.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 21:07:28
 */
public class Engine {

    /**
     * Database's name.
     */
    private String dbName;

    /**
     * Table relation.
     */
    private TreeMap<String, Table> tableMap;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Engine.class.getName());

    /**
     * Create database.
     *
     * @param dbName Database's name.
     * @return
     */
    public boolean createDatabase(String dbName) {
        try {
            String currentPath = System.getProperty("user.dir") + "\\" + dbName;
            this.dbName = dbName;
            File dir = new File(currentPath);
            if (!dir.exists()) {
                return dir.mkdir();
            }
            return true;
        } catch (Exception e) {
            logger.error(e);
            return false;
        }
    }

    /**
     * Create table.
     *
     * @param obj
     * @return
     */
    public boolean createTable(Object obj) {
        if (tableMap == null) {
            tableMap = new TreeMap<>();
        }

        //1. Object's name is table's name.
        String tableName = obj.getClass().getName();
        if (tableMap.containsKey(tableName)) {
            return false;
        }

        Table table = new Table(obj, dbName);
        tableMap.put(tableName, table);
        return true;
    }

    /**
     * Write object to disk.
     *
     * @param obj
     * @return
     */
    public int write(Object obj) {

        //1. Is vaild.
        if (isTableExist(obj) && isVaidObj(obj)) {
            Table table = tableMap.get(obj.getClass().getName());
            if (table != null) {

                // 2. Write to disk. 
                return table.write(obj);
            }
        }
        return -1;
    }

    /**
     * Write many objects to disk.
     *
     * @param objs
     * @return
     */
    public int writeMany(Object[] objs) {
        if (objs == null || objs.length == 0) {
            return -1;
        }
        if (!isTableExist(objs[0])) {
            if (!createTable(objs[0])) {
                return -1;
            }
        }
        Table table = tableMap.get(objs[0].getClass().getName());
        if (table != null) {
            return table.writeMany(objs);
        }
        return -1;
    }

    /**
     * Get total count for an object.
     *
     * @param obj
     * @return
     */
    public int getTotalCount(Object obj) {
        if (obj == null) {
            return -1;
        }
        if (!isTableExist(obj)) {
            if (!createTable(obj)) {
                return -1;
            }
        }
        if (isVaidObj(obj)) {
            Table table = tableMap.get(obj.getClass().getName());
            if (table != null) {
                return table.getTotalCount(obj);
            }
        }
        return -1;
    }

    /**
     * Read an object from disk by index.
     *
     * @param objType
     * @param index
     * @return
     */
    public Object read(Object objType, int index) {
        if (isTableExist(objType) && isVaidObj(objType)) {
            Table table = tableMap.get(objType.getClass().getName());
            if (table != null) {
                return table.read(index);
            }
        }
        return null;
    }

    /**
     * Update object by index.
     *
     * @param index
     * @param newObj
     * @return
     */
    public boolean update(int index, Object newObj) {
        if (isTableExist(newObj) && isVaidObj(newObj)) {
            Table table = tableMap.get(newObj.getClass().getName());
            if (table != null) {
                return table.update(index, newObj);
            }
        }
        return false;
    }

    /**
     * Get database's name.
     *
     * @return
     */
    public String getDBName() {
        return dbName;
    }

    /**
     * Get all tables.
     *
     * @return
     */
    public List<String> getTableNames() {
        String folder = getDBName();
        File file = new File(folder);
        String tablesAndSchemas[];
        tablesAndSchemas = file.list();
        List<String> tableNames = new ArrayList<>();
        if (tablesAndSchemas == null) {
            return tableNames;
        }
        for (String tablesAndSchema : tablesAndSchemas) {
            if (tablesAndSchema.endsWith("schema")) {
                tableNames.add(tablesAndSchema.replace(".schema", ""));
            }
        }
        return tableNames;
    }

    /**
     * Delete table.
     *
     * @param name
     * @return
     */
    public boolean deleteTable(String name) {
        String folder = getDBName();
        File file = new File(folder);
        String tablesAndSchemas[];
        tablesAndSchemas = file.list();
        if (tablesAndSchemas == null) {
            return false;
        }
        boolean isDeleted = true;
        for (String tablesAndSchema : tablesAndSchemas) {
            if (tablesAndSchema.startsWith(name + ".")) {
                File deFile = new File(folder + "/" + tablesAndSchema);
                isDeleted &= deFile.delete();
            }
        }
        if (tableMap.containsKey(name)) {
            tableMap.remove(name);
        }
        return isDeleted;
    }

    /**
     * Backup data.
     *
     * @param backupName
     * @return
     */
    public boolean backup(String backupName) {
        try {
            Folder.copyDirectiory(dbName, backupName);
        } catch (IOException e) {
            return false;
        }
        return true;
    }

    /**
     * Is table exist.
     */
    private boolean isTableExist(Object obj) {
        String name = obj.getClass().getName();
        boolean isContains = tableMap.containsKey(name);
        return isContains;
    }

    /**
     * Is a vaid object.
     *
     * @param obj
     * @return
     */
    private boolean isVaidObj(Object obj) {
        return true;
    }

}
