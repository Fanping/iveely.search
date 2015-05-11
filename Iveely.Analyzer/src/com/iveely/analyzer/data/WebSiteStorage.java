/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.data;

import com.iveely.database.api.DbConnector;
import com.iveely.database.storage.Types;
import java.util.List;
import org.apache.log4j.Logger;

/**
 *
 * @author 凡平
 */
public class WebSiteStorage {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(WebSiteStorage.class.getName());

    private WebSiteStorage() throws Exception {
        init();
    }

    /**
     * Single instance.
     */
    private static WebSiteStorage storage;

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
    public static WebSiteStorage getInstance() {
        if (storage == null) {
            try {
                storage = new WebSiteStorage();
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
     * Get wikipedia url.
     *
     * @param id
     * @return
     */
    public String getUrl(int id) {
        Object[] objs = connector.selectOne(urlTable, id);
        if (objs != null && objs.length == 1) {
            return (String) objs[0];
        }
        return "";
    }

    /**
     * Add many urls.
     *
     * @param urls
     * @return
     */
    public Integer[] addUrls(List<Object[]> urls) {
        return connector.insertMany(urlTable, urls);
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

    public int getPageCount() {
        return connector.getCount(pageTable);
    }

    /**
     * Add many pages.
     *
     * @param pages
     * @return
     */
    public Integer[] addPages(List<Object[]> pages) {
        return connector.insertMany(pageTable, pages);
    }

    /**
     * Add many indexs.
     *
     * @param indexs
     * @return
     */
    public Integer[] addIndexs(List<Object[]> indexs) {
        return connector.insertMany(indexTable, indexs);
    }
}
