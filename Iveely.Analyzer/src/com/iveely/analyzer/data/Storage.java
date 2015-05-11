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
 * @author X1 Carbon
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
     * Url for wikipedia.
     */
    private final String wikiurlTable = "WikiurlSet";

    /**
     * Table name for Image.
     */
    private final String imageTable = "ImageSet";

    /**
     * Table name for Image.
     */
    private final String entityTable = "EntitySet";

    /**
     * Url for bing.
     */
    private final String bingurlTable = "BingurlSet";

    /**
     * Url for hudong baike.
     */
    private final String hudongTable = "HudongSet";

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
        connector = new DbConnector("iveely_base", "127.0.0.1", 4321);

        // 2. Create table.
        boolean isTableCreated = connector.createTable(entityTable,
                new String[]{"Entity", "Relation", "Body", "IsSimple", "ClassType", "DataReference", "UrlReference", "Reliability", "Expired"},
                new Types[]{Types.STRING, Types.STRING, Types.STRING, Types.BOOLEAN, Types.STRING, Types.STRING, Types.STRING, Types.STRING, Types.STRING},
                new boolean[]{false, false, false, false, false, false, false, false, false});

        isTableCreated = isTableCreated && connector.createTable(imageTable,
                new String[]{"Entity", "Image", "Url"},
                new Types[]{Types.STRING, Types.STRING, Types.STRING},
                new boolean[]{false, false, false});

        isTableCreated = isTableCreated && connector.createTable(wikiurlTable,
                new String[]{"Url", "theme"},
                new Types[]{Types.STRING, Types.STRING},
                new boolean[]{true, false});

        isTableCreated = isTableCreated && connector.createTable(bingurlTable,
                new String[]{"Url", "theme"},
                new Types[]{Types.STRING, Types.STRING},
                new boolean[]{true, false});

        isTableCreated = isTableCreated && connector.createTable(hudongTable,
                new String[]{"Url", "theme"},
                new Types[]{Types.STRING, Types.STRING},
                new boolean[]{true, false});

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
    public String getWikiurl(int id) {
        Object[] objs = connector.selectOne(wikiurlTable, id);
        if (objs != null && objs.length == 2) {
            return (String) objs[0];
        }
        return "";
    }

    /**
     * Get many images.
     *
     * @param id
     * @param count
     * @return
     */
    public List<Object[]> getManyImages(int id, int count) {
        return connector.selectMany(imageTable, id, count);
    }

    /**
     * Get many images.
     *
     * @param id
     * @param count
     * @return
     */
    public List<Object[]> getManyEntities(int id, int count) {
        return connector.selectMany(entityTable, id, count);
    }

    /**
     * Get an image.
     *
     * @param id
     * @return
     */
    public Object[] getImage(int id) {
        return connector.selectOne(imageTable, id);
    }

    /**
     * Get wikipedia url.
     *
     * @param id
     * @return
     */
    public String getBingurl(int id) {
        Object[] objs = connector.selectOne(bingurlTable, id);
        if (objs != null && objs.length == 2) {
            return (String) objs[0];
        }
        return "";
    }

    public void addImages(List<Object[]> list) {
        connector.insertMany(imageTable, list);
    }

    /**
     * Get simple knowledge.
     *
     * @param id
     * @return
     */
    public String getKnowledge(int id) {
        Object[] objs = connector.selectOne(entityTable, id);
        if (objs != null && objs.length == 9) {
            if (objs[3] == "true") {
                return objs[0] + " " + objs[1];
            } else {
                return objs[0] + " " + objs[1] + " " + objs[2];
            }
        }
        return "";
    }

    /**
     * Get simple knowledge.
     *
     * @param id
     * @return
     */
    public Object[] getDetailKnowledge(int id) {
        return connector.selectOne(entityTable, id);
    }

    /**
     * Get count of index.
     *
     * @return
     */
    public Integer getImageCount() {
        return connector.getCount(imageTable);
    }

    /**
     * Get count of Entity.
     *
     * @return
     */
    public Integer getEntityCount() {
        return connector.getCount(entityTable);
    }

    /**
     * Get wikipedia url.
     *
     * @param id
     * @return
     */
    public String getHudongurl(int id) {
        Object[] objs = connector.selectOne(hudongTable, id);
        if (objs != null && objs.length == 2) {
            return (String) objs[0];
        }
        return "";
    }

    /**
     * Set wikipedias to database.
     *
     * @param list
     */
    public void setEntity(List<Object[]> list) {
        connector.insertMany(entityTable, list);
    }

    /**
     * Set urls for wikipedias.
     *
     * @param list
     */
    public void setWikiurls(List<Object[]> list) {
        connector.insertMany(wikiurlTable, list);
    }

    /**
     * Set urls for wikipedias.
     *
     * @param list
     */
    public void setBingurls(List<Object[]> list) {
        connector.insertMany(bingurlTable, list);
    }

    /**
     * Set urls for wikipedias.
     *
     * @param list
     */
    public void setHudongurls(List<Object[]> list) {
        connector.insertMany(hudongTable, list);
    }
}
