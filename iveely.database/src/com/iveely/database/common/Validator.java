package com.iveely.database.common;

/**
 * Validation.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-26 22:12:00
 */
public class Validator {

    /**
     * Check name of table and column.
     *
     * @param name
     * @return
     */
    public static boolean isLegal(String name) {
        if (name != null) {
            return name.matches("^(?!_)(?!.*?_$)[a-zA-Z0-9_]+$");
        }
        return false;
    }
}
