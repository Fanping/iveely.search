package com.iveely.database.api;

import com.iveely.database.storage.Types;
import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-11 8:36:27
 */
public class DbTable {

    public DbTable() {
        this.fileds = new ArrayList<>();
    }

    private String name;

    /**
     * @return the name
     */
    public String getName() {
        return name;
    }

    /**
     * @param name the name to set
     */
    public void setName(String name) {
        this.name = name;
    }

    private final List<DbFiled> fileds;

    /**
     * Add filed.
     *
     * @param name
     * @param type
     * @param isUnique
     * @return
     */
    public boolean addFiled(String name, Types type, boolean isUnique) {
        DbFiled filed = new DbFiled();
        filed.setIsUnique(isUnique);
        filed.setName(name);
        return true;
    }
}
