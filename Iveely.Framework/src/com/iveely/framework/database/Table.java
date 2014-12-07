package com.iveely.framework.database;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;

/**
 * The table for store.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 21:54:40
 */
public class Table {

    /**
     * Attributes.
     */
    private final List attributes;

    /**
     * Name of table.
     */
    private final String tableName;

    /**
     * IO writter.
     */
    private final IOBase writter;

    /**
     * Logger
     */
    private static final Logger logger = Logger.getLogger(Table.class.getName());

    public Table(Object obj, String dbName) {
        attributes = new ArrayList();
        tableName = obj.getClass().getSimpleName();

        // 1. Extract attribute.
        Field[] fields = obj.getClass().getDeclaredFields();
        for (Field field : fields) {
            Attribute attribute = new Attribute();
            attribute.setFieldType(field.getType().getSimpleName());
            attribute.setFieldName(field.getName());
            attributes.add(attribute);
        }

        // 2. Build schema.
        buildSchema(dbName);

        // 3. Build IOBase to store. 
        writter = new IOBase(dbName, tableName, obj);
    }

    /**
     * Build Schema.
     *
     * @param dbName
     */
    private void buildSchema(String dbName) {
        try {
            String currentPath = System.getProperty("user.dir") + "\\" + dbName + "\\" + tableName + ".schema";
            File file = new File(currentPath);
            if (!file.exists()) {
                try (OutputStreamWriter out = new OutputStreamWriter(new FileOutputStream(currentPath), "UTF-8")) {
                    for (Object attribute : attributes) {
                        String data = attribute.toString() + "\n";
                        out.write(data);
                    }
                    out.flush();
                }
            }
        } catch (IOException e) {
            logger.warn(e);
        }
    }

    /**
     * Write an object.
     *
     * @param obj Object to write.
     * @return id of the record, -1 is error.
     */
    public int write(Object obj) {
        try {
            return writter.write(obj);
        } catch (IOException | IllegalArgumentException e) {
            logger.error(e);
        }
        return -1;
    }

    /**
     * Write many objects.
     *
     * @param objs
     * @return
     */
    public int writeMany(Object[] objs) {
        try {
            return writter.writeMany(objs);
        } catch (Exception e) {
            logger.error(e);
        }
        return -1;
    }

    /**
     * Read an object by index.
     *
     * @param index
     * @return
     */
    public Object read(int index) {
        return writter.read(index);
    }

    /**
     * Update an object.
     *
     * @param index
     * @param obj
     * @return
     */
    public boolean update(int index, Object obj) {
        return writter.update(index, obj);
    }

    /**
     * Get total count of the table.
     *
     * @param obj
     * @return
     */
    public int getTotalCount(Object obj) {
        return writter.getCount(obj);
    }
}
