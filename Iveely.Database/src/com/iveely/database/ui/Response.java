package com.iveely.database.ui;

import com.iveely.framework.net.websocket.IEventProcessor;
import com.iveely.framework.text.json.JsonObject;
import com.iveely.database.LocalStore;
import com.iveely.database.storage.Warehouse;
import org.apache.log4j.Logger;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-7 23:04:07
 */
public class Response implements IEventProcessor {
    
    private final WebSocketMessage webSocketMessage;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Response.class.getName());
    
    public Response() {
        LocalStore.getWarehouse("system");
        webSocketMessage = new WebSocketMessage();
    }
    
    @Override
    public String invoke(String data) {
        try {
            String database = JsonObject.readFrom(data).get("database").toString().replace("\"", "");
            String command = JsonObject.readFrom(data).get("command").toString().replace("\"", "");
            switch (command) {
                case "show dbs":
                    return showDatabses();
                case "show tbs":
                    return showTables(database);
                case "flush":
                    return flush();
                case "drop":
                    return dropTable(database);
            }
            
        } catch (Exception e) {
            logger.error(e);
        }
        return "";
    }
    
    private String flush() {
        String[] names = LocalStore.getDatabases();
        for (String name : names) {
            LocalStore.getWarehouse(name).close();
        }
        return "";
    }

    /**
     * Show database information.
     *
     * @return
     */
    private String showDatabses() {
        String[] names = LocalStore.getDatabases();
        WebSocketMessage.DatabaseShown databaseShown = webSocketMessage.new DatabaseShown();
        databaseShown.setCommand("show dbs");
        databaseShown.setDbs(names);
        String resp = databaseShown.toJson();
        return resp;
    }
    
    private String dropTable(String dbAndTable) {
        String[] infor = dbAndTable.split(":");
        LocalStore.getWarehouse(infor[0]).dropTable(infor[1]);
        WebSocketMessage.TableDroped tableDroped = webSocketMessage.new TableDroped();
        tableDroped.setCommand("drop");
        return tableDroped.toJson();
    }
    
    private String showTables(String dbName) {
        if (LocalStore.isWarehouseExist(dbName)) {
            Warehouse warehouse = LocalStore.getWarehouse(dbName);
            String[] names = warehouse.getTableNames();
            Integer[] counter = new Integer[names.length];
            for (int i = 0; i < names.length; i++) {
                counter[i] = warehouse.count(names[i]);
            }
            WebSocketMessage.TableShown tableShown = webSocketMessage.new TableShown();
            tableShown.setCommand("show tbs");
            tableShown.setNames(names);
            tableShown.setCounter(counter);
            String resp = tableShown.toJson();
            return resp;
        } else {
            return "[\"error\":\"Data base is not exist.\"]";
        }
    }
}
