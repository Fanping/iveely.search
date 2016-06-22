package com.iveely.database;

import com.iveely.database.message.CloseStatus;
import com.iveely.database.message.OpenStatus;

/**
 * Connector for database.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-26 20:38:20
 */
public class Connector {

    /**
     * The server to connect.
     */
    private String server;

    /**
     * @return the server
     */
    public String getServer() {
        return server;
    }

    /**
     * Open connection.
     *
     * @param server
     * @param port
     * @param dbName
     * @return
     */
    public OpenStatus open(String server, String port, String dbName) {
        this.server = server;
        return OpenStatus.NOT_FOUND_HOST;
    }

    /**
     * Close connection.
     *
     * @return
     */
    public CloseStatus close() {
        return CloseStatus.SUCCESS;
    }
}
