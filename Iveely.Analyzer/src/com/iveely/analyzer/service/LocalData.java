/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.service;

import com.iveely.database.LocalStore;
import com.iveely.database.storage.Types;
import com.iveely.database.storage.Warehouse;
import java.util.List;

/**
 *
 * @author X1 Carbon
 */
public class LocalData {

    /**
     * House Name of Local data.
     */
    private final String houseName;

    /**
     * Current warehouse.
     */
    private final Warehouse warehouse;

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
     * Table for store index.
     */
    private final String indexTable = "Index";

    /**
     * Table for suggest.
     */
    private final String suggestTable = "Suggest";

    private LocalData() {
        this.houseName = "iveely_base";
        this.warehouse = LocalStore.getWarehouse(houseName);
    }

    private static LocalData localData;

    public static LocalData getInstance() {
        if (localData == null) {
            localData = new LocalData();
            localData.init();
        }
        return localData;
    }

    private void init() {

        // 1. Create entity table.
        warehouse.createTable(entityTable);
        warehouse.createColumn(entityTable, "Entity", Types.STRING, false);
        warehouse.createColumn(entityTable, "Relation", Types.STRING, false);
        warehouse.createColumn(entityTable, "Body", Types.STRING, false);
        warehouse.createColumn(entityTable, "IsSimple", Types.STRING, false);
        warehouse.createColumn(entityTable, "ClassType", Types.STRING, false);
        warehouse.createColumn(entityTable, "DataReference", Types.STRING, false);
        warehouse.createColumn(entityTable, "UrlReference", Types.STRING, false);
        warehouse.createColumn(entityTable, "Reliability", Types.STRING, false);
        warehouse.createColumn(entityTable, "Expired", Types.STRING, false);

        // 2. Create image table.
        warehouse.createTable(imageTable);
        warehouse.createColumn(imageTable, "Entity", Types.STRING, false);
        warehouse.createColumn(imageTable, "Image", Types.STRING, false);
        warehouse.createColumn(imageTable, "Url", Types.STRING, false);

        // 3. Create index table.
        warehouse.createTable(indexTable);
        warehouse.createColumn(indexTable, "key", Types.STRING, false);
        warehouse.createColumn(indexTable, "docId", Types.STRING, false);
        warehouse.createColumn(indexTable, "rank", Types.STRING, false);
    }

    /**
     * Add entities.
     *
     * @param list
     * @return
     */
    public Integer addEntities(List<Object[]> list) {
        return warehouse.bulkInsert(entityTable, list);
    }

    /**
     * Add entity.
     *
     * @param data
     * @return
     */
    public Integer addEntity(Object[] data) {
        return warehouse.insert(entityTable, data);
    }

    /**
     * Add entities.
     *
     * @param list
     * @return
     */
    public Integer addImages(List<Object[]> list) {
        return warehouse.bulkInsert(imageTable, list);
    }

    /**
     * Add indexs.
     *
     * @param list
     * @return
     */
    public Integer addIndexs(List<Object[]> list) {
        return warehouse.bulkInsert(indexTable, list);
    }

    /**
     * Get image.
     *
     * @param id
     * @return
     */
    public Object[] getImage(Integer id) {
        return warehouse.selectById(imageTable, id);
    }

    /**
     * Get entity by id.
     *
     * @param id
     * @return
     */
    public Object[] getEntity(Integer id) {
        return warehouse.selectById(entityTable, id);
    }
}
