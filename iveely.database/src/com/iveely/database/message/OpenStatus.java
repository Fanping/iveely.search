package com.iveely.database.message;

/**
 * Open status for connector.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-26 20:46:06
 */
public enum OpenStatus {

    /*
     Open connection success.
     */
    SUCCESS,
    /**
     * Not found host,
     */
    NOT_FOUND_HOST,
    /**
     * Not found the database.
     */
    NOT_FOUND_DATABASE
}
