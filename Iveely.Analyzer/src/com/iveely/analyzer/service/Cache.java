/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.service;

import com.iveely.framework.text.json.JsonObject;
import com.iveely.analyzer.data.DynamicIndex;
import com.iveely.analyzer.data.Storage;
import com.iveely.analyzer.data.TermInverted;
import com.iveely.analyzer.service.template.JsonDetail;
import com.iveely.analyzer.service.template.JsonImage;
import com.iveely.analyzer.service.template.JsonRecommend;
import com.iveely.analyzer.service.template.JsonSearch;
import com.iveely.analyzer.service.template.JsonSuggest;
import com.iveely.database.common.Configurator;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 *
 * @author X1 Carbon
 */
public class Cache implements Serializable {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Cache.class.getName());

    /**
     * Local cache file.
     */
    private final String localCache = "cache.c";
    
    private final HashMap<Integer, DynamicIndex> _result;

    /**
     * Stop words.
     */
    private final HashSet<String> stopWords;
    
    public Cache() {
        this.suggestions = new ArrayList<>();
        this.images = new HashMap<>();
        this._result = new HashMap<>();
        this.stopWords = new HashSet<>();
        this.imageId = 0;
        this.entityId = 0;
        if (this.stopWords.isEmpty()) {
            this.stopWords.add(" ");
            this.stopWords.add("?");
            this.stopWords.add("？");
            this.stopWords.add("。");
            this.stopWords.add(".");
            this.stopWords.add("!");
            this.stopWords.add("、");
            this.stopWords.add("（");
            this.stopWords.add("）");
            this.stopWords.add("，");
        }
    }

    /**
     * Knowledge search index.
     */
    private final HashMap<Integer, Integer> images;

    /**
     * All suggestions.
     */
    private List<Integer> suggestions;

    /**
     * Suggest id.
     */
    private Integer entityId;

    /**
     * Image id.
     */
    private Integer imageId;

    /**
     * Load information.
     */
    public void load() {
        // 1. Load image.
        loadImage();

        // 2. Load index
        loadEntity();
    }
    
    public String query(String queryJson) {
        String queryContext = JsonObject.readFrom(queryJson).get("query").toString().replace("\"", "");
        String[] terms = com.iveely.framework.segment.WordBreaker.getInstance().splitToArray(queryContext);

        // 1. Get all docs.
        HashMap<Integer, Double> weigths = new HashMap<>();
        Double maxValue = Double.MIN_VALUE;
        for (String term : terms) {
            Integer code = term.hashCode();
            if (this._result.containsKey(code)) {
                List<TermInverted> ranks = ((DynamicIndex) this._result.get(code)).getTermInverteds();
                for (TermInverted rank : ranks) {
                    if (weigths.containsKey(rank.getPage())) {
                        Double val = (weigths.get(rank.getPage()) + rank.getRank()) * 2;
                        weigths.put(rank.getPage(), val);
                        if (maxValue < val) {
                            maxValue = val;
                        }
                    } else {
                        if (rank.getRank() > maxValue) {
                            maxValue = rank.getRank();
                        }
                        weigths.put(rank.getPage(), rank.getRank());
                    }
                }
            }
        }

        // 2. Sort docs.
        ArrayList keys = new ArrayList(weigths.keySet());
        Collections.sort(keys, new Comparator<Object>() {
            @Override
            public int compare(Object o1, Object o2) {
                if (Double.parseDouble(weigths.get(o1).toString()) < Double.parseDouble(weigths.get(o2).toString())) {
                    return 1;
                } else if (Double.parseDouble(weigths.get(o1).toString()) == Double.parseDouble(weigths.get(o2).toString())) {
                    return 0;
                } else {
                    return -1;
                }
            }
        }
        );

        // 3. Get documents.
        int limitCount = 10;
        Iterator iter = weigths.entrySet().iterator();
        HashMap<Integer, Object> imageCache = new HashMap<>();
        List<Object[]> list = new ArrayList<>();
        while (iter.hasNext() && limitCount > 0) {
            Map.Entry entry = (Map.Entry) iter.next();
            Integer docId = (Integer) entry.getKey();
            Double rank = (Double) entry.getValue() / (maxValue + 1);
            Object[] objs = LocalData.getInstance().getEntity(docId);
            if (objs != null && objs.length == 9) {
                Object[] effObjs = new Object[13];
                String similar = objs[0] + "" + objs[1];
                System.arraycopy(objs, 0, effObjs, 0, 9);
                effObjs[9] = rank;
                Integer imageCode = ((String) objs[0]).hashCode();
                if (images.containsKey(imageCode)) {
                    Integer imageLocation = images.get(imageCode);
                    if (imageCache.containsKey(imageLocation)) {
                        effObjs[10] = imageCache.get(imageLocation);
                    } else {
                        Object[] imgObj = LocalData.getInstance().getImage(imageLocation);
                        if (imgObj.length == 3) {
                            effObjs[10] = imgObj[1];
                            imageCache.put(imageLocation, imgObj[1]);
                        } else {
                            effObjs[10] = "";
                        }
                    }
                } else {
                    effObjs[10] = "";
                }
                effObjs[11] = docId + "";
                effObjs[12] = com.iveely.framework.text.Document.getSimilarity(queryContext, similar) + "";
                limitCount--;
                list.add(effObjs);
            }
        }

        // 4. Build json.
        JsonSearch jsonSearch = new JsonSearch();
        jsonSearch.setType("Success");
        jsonSearch.setCommond("search");
        jsonSearch.setData(list);
        String result = jsonSearch.toJson();
        return result;
    }
    
    public String suggest(String queryJson) {
        String queryContext = JsonObject.readFrom(queryJson).get("query").toString().replace("\"", "");
        String[] terms = com.iveely.framework.segment.WordBreaker.getInstance().splitToArray(queryContext);

        // 1. Get all docs.
        HashMap<Integer, Double> weigths = new HashMap<>();
        Double maxValue = Double.MIN_VALUE;
        for (String term : terms) {
            Integer code = term.hashCode();
            if (this._result.containsKey(code)) {
                List<TermInverted> ranks = ((DynamicIndex) this._result.get(code)).getTermInverteds();
                for (TermInverted rank : ranks) {
                    if (weigths.containsKey(rank.getPage())) {
                        Double val = (weigths.get(rank.getPage()) + rank.getRank()) * 2;
                        weigths.put(rank.getPage(), val);
                        if (maxValue < val) {
                            maxValue = val;
                        }
                    } else {
                        if (rank.getRank() > maxValue) {
                            maxValue = rank.getRank();
                        }
                        weigths.put(rank.getPage(), rank.getRank());
                    }
                }
            }
        }

        // 2. Sort docs.
        ArrayList keys = new ArrayList(weigths.keySet());
        Collections.sort(keys, new Comparator<Object>() {
            @Override
            public int compare(Object o1, Object o2) {
                if (Double.parseDouble(weigths.get(o1).toString()) < Double.parseDouble(weigths.get(o2).toString())) {
                    return 1;
                } else if (Double.parseDouble(weigths.get(o1).toString()) == Double.parseDouble(weigths.get(o2).toString())) {
                    return 0;
                } else {
                    return -1;
                }
            }
        }
        );

        // 3. Get documents.
        int limitCount = 10;
        Iterator iter = weigths.entrySet().iterator();
        HashMap<Integer, Object> imageCache = new HashMap<>();
        List<Object[]> list = new ArrayList<>();
        while (iter.hasNext() && limitCount > 0) {
            Map.Entry entry = (Map.Entry) iter.next();
            Integer docId = (Integer) entry.getKey();
            Double rank = (Double) entry.getValue() / (maxValue + 1);
            Object[] objs = LocalData.getInstance().getEntity(docId);
            if (objs != null && objs.length == 9) {
                Object[] effObjs = new Object[4];
                effObjs[0] = objs[0];
                effObjs[1] = objs[1];
                effObjs[2] = rank;
                effObjs[3] = docId + "";
                limitCount--;
                list.add(effObjs);
            }
        }

        // 4. Build json.
        JsonSuggest jsonSuggest = new JsonSuggest();
        jsonSuggest.setType("Success");
        jsonSuggest.setCommond("suggest");
        jsonSuggest.setData(list);
        String result = jsonSuggest.toJson();
        return result;
    }
    
    public String recommend(String queryJson) {
        String requestId = JsonObject.readFrom(queryJson).get("requestid").toString().replace("\"", "");
        int id = Integer.parseInt(requestId);
        List<Object[]> infors = new ArrayList<>();
        int base = id / 10;
        int index = 1;
        for (int i = 0; i < base; i++) {
            index *= 10;
        }
        int f = id % 10 + index;
        HashSet<Integer> repeat = new HashSet<>();
        for (int i = 0; i < 10; i++) {
            int flag = f + i * 10;
            if (flag >= suggestions.size()) {
                break;
            }
            int realId = suggestions.get(f + i * 10);
            Object[] knowledge = LocalData.getInstance().getEntity(realId);
            if (knowledge != null) {
                infors.add(knowledge);
            }
        }
        
        if (!infors.isEmpty()) {
            JsonRecommend recommend = new JsonRecommend();
            recommend.setType("Success");
            recommend.setCommond("recommend");
            recommend.setData(infors);
            return recommend.toJson();
        } else {
            JsonRecommend recommend = new JsonRecommend();
            recommend.setType("falure");
            recommend.setCommond("recommend");
            recommend.setData(infors);
            return recommend.toJson();
        }
    }
    
    public String detail(String queryJson) {
        String requestId = JsonObject.readFrom(queryJson).get("requestid").toString().replace("\"", "");
        int id = Integer.parseInt(requestId);
        Object[] objs = LocalData.getInstance().getEntity(id);
        JsonDetail jsonDetail = new JsonDetail();
        jsonDetail.setCommond("detail");
        if (objs != null) {
            jsonDetail.setType("Success");
            jsonDetail.setData(objs);
        } else {
            jsonDetail.setType("Error");
            jsonDetail.setData(new Object[]{"Not found."});
        }
        return jsonDetail.toJson();
    }
    
    public String image(String queryJson) {
        String queryContext = JsonObject.readFrom(queryJson).get("query").toString().replace("\"", "");
        Integer code = queryContext.hashCode();
        JsonImage jsonImage = new JsonImage();
        jsonImage.setCommond("image");
        if (images.containsKey(code)) {
            Object[] obj = LocalData.getInstance().getImage((Integer) images.get(code));
            jsonImage.setType("Success");
            jsonImage.setData((String) obj[1]);
        } else {
            jsonImage.setType("Falure");
            jsonImage.setData("Not found the image.");
        }
        return jsonImage.toJson();
    }

    /**
     * Load image. "00:00 - 6:00"
     */
    private void loadImage() {
        Integer count = Storage.getInstance().getImageCount() - this.imageId;
        try {
            Integer step = 50;
            Integer size = count / step;
            for (int i = 0; i < size && (Calendar.getInstance().get(Calendar.HOUR_OF_DAY) >= 0 && Calendar.getInstance().get(Calendar.HOUR_OF_DAY) < 6); i++) {
                List<Object[]> list = Storage.getInstance().getManyImages(this.imageId + i * step, step);
                if (list == null) {
                    continue;
                }
                if (list.size() > 0) {
                    Integer id = LocalData.getInstance().addImages(list);
                    for (int j = 0; j < list.size(); j++) {
                        String encodeName = java.net.URLDecoder.decode((String) list.get(j)[0], "utf-8");
                        images.put(encodeName.hashCode(), id - 1);
                        this.imageId++;
                    }
                }
            }
            if (Calendar.getInstance().get(Calendar.HOUR_OF_DAY) >= 0 && Calendar.getInstance().get(Calendar.HOUR_OF_DAY) < 6) {
                List<Object[]> list = Storage.getInstance().getManyImages(this.imageId + size * step, count % step);
                if (list.size() > 0) {
                    Integer id = LocalData.getInstance().addImages(list);
                    for (int j = 0; j < list.size(); j++) {
                        String encodeName = java.net.URLDecoder.decode((String) list.get(j)[0], "utf-8");
                        images.put(encodeName.hashCode(), id - 1);
                        this.imageId++;
                    }
                }
            }
        } catch (Exception e) {
            logger.error(e);
        }
        this.imageId += count;
        Configurator.save(localCache, this);
    }

    /**
     * Load term invert. "6:00~23:00"
     */
    public void loadEntity() {
        Integer count = Storage.getInstance().getEntityCount();
        try {
            Integer step = 200;
            Integer size = count / step;
            for (int i = 0; i < size && (Calendar.getInstance().get(Calendar.HOUR_OF_DAY) >= 6 && Calendar.getInstance().get(Calendar.HOUR_OF_DAY) < 23); i++) {
                
                List<Object[]> list = Storage.getInstance().getManyEntities(i * step, step);
                if (list == null) {
                    continue;
                }
                for (Object[] obj : list) {
                    Integer id = LocalData.getInstance().addEntity(obj);
                    if (obj[3] == "false" && id != -1) {
                        suggestions.add(id);
                        processIndex(obj[0] + "" + obj[1] + "" + obj[2], id);
                    } else if (id > -1) {
                        processIndex(obj[0] + "" + obj[1], id);
                    }
                }
                
            }
            if (Calendar.getInstance().get(Calendar.HOUR_OF_DAY) >= 6 && Calendar.getInstance().get(Calendar.HOUR_OF_DAY) < 23) {
                List<Object[]> list = Storage.getInstance().getManyEntities(size * step, count % step);
                for (Object[] obj : list) {
                    Integer id = LocalData.getInstance().addEntity(obj);
                    if (obj[3] == "false" && id != -1) {
                        suggestions.add(id);
                        processIndex(obj[0] + "" + obj[1] + "" + obj[2], id);
                    } else if (id > -1) {
                        processIndex(obj[0] + "" + obj[1], id);
                    }
                    
                }
            }
        } catch (Exception e) {
            logger.error(e);
        }
        this.entityId += count;
        Configurator.save(localCache, this);
    }
    
    private void processIndex(String sentence, Integer id) {
        //1. Split words.
        String[] words = com.iveely.framework.segment.WordBreaker.getInstance().splitToArray(sentence);
        if (words == null || words.length == 0) {
            return;
        }
        TreeMap<String, Double> counter = new java.util.TreeMap<>();
        int totalCount = words.length;
        for (String keyword : words) {
            if (this.stopWords.contains(keyword)) {
                continue;
            }
            if (counter.containsKey(keyword)) {
                double rank = counter.get(keyword);
                counter.put(keyword, (rank + 1.0) / totalCount);
            } else {
                counter.put(keyword, (1.0 / totalCount));
            }
        }

        //2. Build list of Indexs.
        List<Object[]> list = new ArrayList<>();
        Iterator<String> iterator = counter.keySet().iterator();
        while (iterator.hasNext()) {
            String keyword = iterator.next();
            double rank = counter.get(keyword) * keyword.length();
            Integer key = keyword.hashCode();
            if (_result.containsKey(key)) {
                DynamicIndex dynamicIndex = (DynamicIndex) _result.get(key);
                TermInverted inverted = new TermInverted();
                inverted.setPage(id);
                inverted.setTerm(key);
                inverted.setRank(rank);
                dynamicIndex.addTermIndex(inverted);
            } else {
                DynamicIndex dynamicIndex = new DynamicIndex();
                TermInverted inverted = new TermInverted();
                inverted.setPage(id);
                inverted.setTerm(key);
                inverted.setRank(rank);
                dynamicIndex.addTermIndex(inverted);
                _result.put(key, dynamicIndex);
            }
        }
    }
    
}
