package iveely.search.store;

import com.iveely.framework.database.Engine;

/**
 * User Data Acquisition.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-2 10:42:32
 */
public class UserLogger {

    /**
     * Data storage engine.
     */
    private Engine engine;

    /**
     * Single instance.
     */
    private static UserLogger userData;

    /**
     * The name of database.
     */
    private static String dbName;

    private UserLogger() {
        if (engine == null) {
            engine = new Engine();
            engine.createDatabase(dbName);
            engine.createTable(new UserClick());
        }
    }

    public static void init(String name) {
        dbName = name;
    }

    /**
     * Get single instance.
     *
     * @return
     */
    public static UserLogger getInstance() {
        if (userData == null) {
            userData = new UserLogger();
        }
        return userData;
    }

    /**
     * Add user log.
     *
     * @param obj
     * @return
     */
    public int addClickLog(UserClick obj) {
        return engine.write(obj);
    }
}
