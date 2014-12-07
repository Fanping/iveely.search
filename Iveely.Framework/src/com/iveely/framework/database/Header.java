package com.iveely.framework.database;

/**
 * Header of the table.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 22:04:51
 */
public class Header {

    /**
     * Get size of table's name.
     *
     * @return
     */
    public static int getTableNameSize() {
        return 50;
    }

    /**
     * Get size of table's cloumn.
     *
     * @return
     */
    public static int getCloumnSize() {
        return 50;
    }

    /**
     * Get size of last record.
     *
     * @return
     */
    public static int getLastRecordSize() {
        return 4;
    }

    /**
     * The max record size of a file.
     *
     * @return
     */
    public static int getMaxRecoredSize() {
        return 10000000;
    }

    /**
     * Get max size of a database.
     *
     * @return
     */
    public static int getMaxMemorySize() {
        return 524288000; //500M
    }
}
