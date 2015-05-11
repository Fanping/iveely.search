/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.plugins.pagesearch.data;

import com.iveely.database.api.DbConnector;
import com.iveely.database.storage.Types;
import java.util.List;
import org.apache.log4j.Logger;

/**
 *
 * @author 凡平
 */
public class Storage {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Storage.class.getName());

    private Storage() throws Exception {
        init();
    }

    /**
     * Single instance.
     */
    private static Storage storage;

    /**
     * Database connect.
     */
    private DbConnector connector;

    /**
     * Url for website.
     */
    private final String urlTable = "WebSite_Urls";

    /**
     * Page for website.
     */
    private final String pageTable = "WebSite_Pages";

    /**
     * Index for website.
     */
    private final String indexTable = "WebSite_Indexs";

    /**
     * Get instance of the Storeage.
     *
     * @return
     */
    public static Storage getInstance() {
        if (storage == null) {
            try {
                storage = new Storage();
            } catch (Exception e) {
                logger.error(e);
                return null;
            }
        }
        return storage;
    }

    public void close() {
        connector.close();
    }

    private void init() throws Exception {
        // 1. Create connector.
        connector = new DbConnector("iveely_sites", "127.0.0.1", 4321);

        // 2. Create table.
        boolean isTableCreated = connector.createTable(pageTable,
                new String[]{"Title", "Url", "Content", "IsDomain", "IsHost", "LogoUrl", "PublishDate", "Expired"},
                new Types[]{Types.STRING, Types.STRING, Types.STRING, Types.BOOLEAN, Types.BOOLEAN, Types.STRING, Types.STRING, Types.STRING},
                new boolean[]{false, false, false, false, false, false, false, false});

        isTableCreated = isTableCreated && connector.createTable(urlTable,
                new String[]{"Url"},
                new Types[]{Types.STRING},
                new boolean[]{true});

        isTableCreated = isTableCreated && connector.createTable(indexTable,
                new String[]{"Term", "PageId", "Weight"},
                new Types[]{Types.INTEGER, Types.STRING, Types.DOUBLE},
                new boolean[]{false, false, false});

        if (!isTableCreated) {
            throw new Exception("Database error.");
        }
    }

    /**
     * Get page by id.
     *
     * @param id
     * @return
     */
    public Object[] getPage(int id) {
        return connector.selectOne(pageTable, id);
    }

    /**
     * Get count of page.
     *
     * @return
     */
    public int getPageCount() {
        return connector.getCount(pageTable);
    }

    /**
     * Get count of index.
     *
     * @return
     */
    public int getIndexCount() {
        return connector.getCount(indexTable);
    }

    /**
     * Get many indexs.
     *
     * @param start
     * @param count
     * @return
     */
    public List<Object[]> getIndexs(int start, int count) {
        return connector.selectMany(indexTable, start, count);
    }

}
