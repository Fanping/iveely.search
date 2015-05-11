/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.researcher;

import com.iveely.analyzer.data.WebSiteStorage;
import com.iveely.computing.api.FieldsDeclarer;
import com.iveely.computing.api.IInput;
import com.iveely.computing.api.IOutput;
import com.iveely.computing.api.StreamChannel;
import com.iveely.computing.api.TopologyBuilder;
import com.iveely.computing.api.TopologySubmitter;
import com.iveely.computing.api.Tuple;
import com.iveely.framework.text.HashCounter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * Index websites.
 *
 * @author 凡平
 */
public class Indexer {

    /**
     * Provide page to tools.
     */
    public static class PageProvider extends IInput {

        /**
         * Logger.
         */
        private static final Logger logger = Logger.getLogger(PageProvider.class.getName());

        /**
         * Output data to collector.
         */
        private StreamChannel _channel;

        @Override
        public void start(HashMap<String, Object> conf, StreamChannel collector) {
            this._channel = collector;
        }

        @Override
        public void declareOutputFields(FieldsDeclarer declarer) {
            declarer.declare(new String[]{"id", "url", "title", "content"}, new Integer[]{0});
        }

        @Override
        public void nextTuple() {
            Object[] objs = getPage();
            if (objs != null && objs.length == 8) {
                this._channel.emit(new Object[]{objs[objs.length - 1], objs[1], objs[0], objs[2]});
            } else {
                try {
                    Thread.sleep(1000 * 5);
                } catch (Exception e) {
                    logger.error(e);
                }
            }
        }

        @Override
        public void toOutput() {
            _channel.addOutputTo(new IndexTool());
        }

        @Override
        public void end(HashMap<String, Object> conf) {

        }

        /**
         * Get page from iveely-db.
         *
         * @return
         */
        private Object[] getPage() {
            Object[] obj = null;
            Integer intId = 0;
            int totalCount = WebSiteStorage.getInstance().getPageCount();
            try {
                String strId = getPublicCache("index_id");
                if (strId.isEmpty()) {
                    setPublicCache("index_id", "1");
                    obj = WebSiteStorage.getInstance().getPage(0);
                    intId = 0;
                } else {
                    intId = Integer.parseInt(strId);
                    if (intId < totalCount) {
                        setPublicCache("index_id", (intId + 1) + "");
                        obj = WebSiteStorage.getInstance().getPage(intId);
                    } else {
                        obj = new Object[1];
                        obj[0] = intId;
                    }
                }
            } catch (Exception e) {
                logger.error(e);
            }
            if (obj != null && obj.length == 8) {
                obj[obj.length - 1] = intId;
            }
            return obj;
        }

    }

    /**
     * Index pages.
     */
    public static class IndexTool extends IOutput {

        /**
         * Logger.
         */
        private static final Logger logger = Logger.getLogger(IndexTool.class.getName());

        /**
         * Urls cache to write.
         */
        private final List<Object[]> termWeight = new ArrayList<>();

        /**
         * Output data to collector.
         */
        private StreamChannel _channel;

        @Override
        public void start(HashMap<String, Object> conf, StreamChannel collector) {
            this._channel = collector;
        }

        @Override
        public void declareOutputFields(FieldsDeclarer declarer) {
            declarer.declare(new String[]{"term", "docId", "weight"}, new Integer[]{0});
        }

        @Override
        public void execute(Tuple tuple) {
            try {
                if (tuple.getSize() != 4) {
                    return;
                }
                // 1. Get elements.
                String id = (String) tuple.get(0);
                String url = (String) tuple.get(1);
                String title = (String) tuple.get(2);
                String content = (String) tuple.get(3);

                // 2. Split.
                title = title.contains("-") ? title.substring(0, title.lastIndexOf("-")) : title;
                String[] titleWords = com.iveely.framework.segment.DicSegment.getInstance().split(title, true);
                TreeMap<Integer, Double> titleMap = HashCounter.statistic(titleWords);
                String[] contentWords = com.iveely.framework.segment.DicSegment.getInstance().split(content, false);
                TreeMap<Integer, Double> contentMap = HashCounter.statistic(contentWords);

                // 3. Combine.
                Iterator iter = titleMap.entrySet().iterator();
                while (iter.hasNext()) {
                    Map.Entry entry = (Entry) iter.next();
                    Integer key = (Integer) entry.getKey();
                    Double value = (Double) entry.getValue();
                    Object[] obj = new Object[3];
                    obj[0] = key;
                    obj[1] = id;
                    if (contentMap.containsKey(key)) {
                        obj[2] = contentMap.get(key) + value * 3;
                    } else {
                        obj[2] = value * 3;
                    }
                    termWeight.add(obj);
                }

                // 4. Insert to database.
                if (termWeight.size() > 0) {
                    WebSiteStorage.getInstance().addIndexs(termWeight);
                    termWeight.clear();
                }
            } catch (Exception e) {
                e.printStackTrace();
                logger.error(e);
            }
        }

        @Override
        public void toOutput() {

        }

        @Override
        public void end(HashMap<String, Object> conf) {
            if (termWeight.size() > 0) {
                WebSiteStorage.getInstance().addIndexs(termWeight);
                termWeight.clear();
            }
        }
    }

    public static void main2(String[] args) {
        TopologyBuilder builder = new TopologyBuilder("PageIndexTopology");
        builder.setInput(new PageProvider(), 1);
        builder.setSlave(1);
        builder.isLocalMode = false;
        builder.setOutput(new IndexTool(), 2);
        TopologySubmitter.submit(builder, args);
    }
}
