/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.plugins.pagesearch.data;

import com.iveely.database.common.Configurator;
import com.iveely.framework.text.json.JsonUtil;
import com.sun.javafx.scene.control.skin.VirtualFlow;
import com.sun.org.apache.xalan.internal.xsltc.compiler.util.Type;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 *
 * @author 凡平
 */
public class Cache implements Runnable, Serializable {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Cache.class.getName());

    /**
     * Index id to get index.
     */
    private int indexId;

    /**
     * Last index id.
     */
    private int lastId;

    /**
     * Terms of all keywords.
     */
    private TreeMap<Integer, DynamicIndex> terms;

    public Cache() {
        terms = new TreeMap<>();
        indexId = 0;
        lastId = 0;
    }

    public List<Object[]> search(String query, String[] words) {
        // 1. Get all pages.
        int size = words.length;
        List<TermInverted> list = new ArrayList<>();
        for (int i = 0; i < size; i++) {
            int code = words[i].hashCode();
            if (terms.containsKey(code)) {
                DynamicIndex di = terms.get(code);
                list.addAll(di.getTermInverteds());
            }
        }

        // 2. Remove duplicate values.
        HashMap<Integer, Double> singleIndex = new HashMap<>();
        list.stream().forEach((inverted) -> {
            if (singleIndex.containsKey(inverted.getPage())) {
                Double rank = singleIndex.get(inverted.getPage()) + inverted.getRank() * 6.16798;
                singleIndex.put(inverted.getPage(), rank);
            } else {
                singleIndex.put(inverted.getPage(), inverted.getRank());
            }
        });

        // 3. Sort top pages.
        List<Map.Entry<Integer, Double>> list_Data = new ArrayList<>(singleIndex.entrySet());
        Collections.sort(list_Data, (Map.Entry<Integer, Double> o1, Map.Entry<Integer, Double> o2) -> {
            if ((o2.getValue() - o1.getValue()) > 0) {
                return 1;
            } else if ((o2.getValue() - o1.getValue()) == 0) {
                return 0;
            } else {
                return -1;
            }
        });

        // 4. Get pages.
        int topCount = 5;
        HashSet<String> dups = new HashSet<>();
        List<Object[]> result = new ArrayList<>();
        for (Entry<Integer, Double> e : list_Data) {
            int docId = e.getKey();
            Object[] obj = Storage.getInstance().getPage(docId);
            if (obj == null && dups.contains((String)obj[1])) {
                continue;
            }
            Object[] robj = new Object[obj.length + 1];
            for (int m = 0; m < obj.length; m++) {
                robj[m] = obj[m];
            }
            robj[obj.length] = docId;
            topCount--;
            result.add(robj);
            dups.add((String)obj[1]);
            if (topCount < 0) {
                break;
            }
        }
        return result;
    }

    @Override
    public void run() {
        while (true) {
            indexId = Storage.getInstance().getIndexCount();
            logger.info("last id = " + lastId + ", index id = " + indexId);
            if (indexId != lastId) {
                int blockCount = indexId / 100;
                int leftCount = indexId % 100;
                for (int i = lastId; i < blockCount; i++) {
                    List<Object[]> list = Storage.getInstance().getIndexs(i * 100, 100);
                    processIndex(list);
                    logger.info(i + "/" + blockCount);
                }
                if (leftCount > 0) {
                    List<Object[]> list = Storage.getInstance().getIndexs(blockCount * 100, leftCount);
                    processIndex(list);
                }
                lastId = indexId;
            } else {
                logger.info("No update data.");
            }

            try {
                logger.info("Write to cache file.");
                Configurator.save("pagesearch.c", this);
                Thread.sleep(1000 * 60 * 60 * 2);
            } catch (Exception e) {
                logger.error(e);
            }
        }
    }

    private void processIndex(List<Object[]> list) {
        if (list != null) {
            int size = list.size();
            for (int i = 0; i < size; i++) {
                Object[] obj = list.get(i);
                if (obj != null && obj.length == 3) {
                    Integer key = Integer.parseInt((String) obj[0]);
                    Integer docId = Integer.parseInt((String) obj[1]);
                    Double weight = Double.parseDouble((String) obj[2]);
                    TermInverted inverted = new TermInverted();
                    inverted.setPage(docId);
                    inverted.setRank(weight);
                    inverted.setTerm(key);
                    if (terms.containsKey(key)) {
                        DynamicIndex di = terms.get(key);
                        di.addTermIndex(inverted);
                    } else {
                        DynamicIndex di = new DynamicIndex();
                        di.addTermIndex(inverted);
                        terms.put(key, di);
                    }
                }
            }
        }
    }

    /**
     * Change color of the text.
     *
     * @param keys
     * @param result
     * @return
     */
    private String changeColor(String[] keys, String result) {
        for (String key : keys) {
            if (!"".equals(key)) {
                result = result.replace(key, "<font color='#c00'>" + key + "</font>");
            }
        }
        return result;
    }

}
