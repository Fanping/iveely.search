package com.iveely.database.api;

import com.iveely.database.storage.Types;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-11 8:37:19
 */
public class DbFiled {

    /**
     * Name of the filed.
     */
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

    private boolean isUnique;

    /**
     * @return the isUnique
     */
    public boolean isIsUnique() {
        return isUnique;
    }

    /**
     * @param isUnique the isUnique to set
     */
    public void setIsUnique(boolean isUnique) {
        this.isUnique = isUnique;
    }
    
    /**
     * Type of this field.
     */
    private Types type;

    /**
     * @return the type
     */
    public Types getType() {
        return type;
    }

    /**
     * @param type the type to set
     */
    public void setType(Types type) {
        this.type = type;
    }
}
