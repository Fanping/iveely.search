package iveely.search.service;

import com.iveely.framework.file.Reader;
import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import iveely.search.store.HtmlPage;
import iveely.search.store.Image;
import iveely.search.store.ImageDatabase;
import iveely.search.store.KTermLocation;
import iveely.search.store.KnowledgeBase;
import iveely.search.store.KnowledgeIndex;
import iveely.search.store.TermInverted;
import iveely.search.store.TermLocation;
import iveely.search.store.TextDatabae;
import iveely.search.store.UserClick;
import iveely.search.store.UserLogger;
import iveely.search.store.Wikipedia;
import iveely.search.store.WikipediaLocation;
import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Calendar;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;
import java.util.TreeSet;
import org.apache.log4j.Logger;

/**
 * Slave callback.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-1 18:20:26
 */
public class SCallback implements ICallback, Runnable {

    /**
     * Reverse file Frequency.
     */
    private static TreeMap<Integer, Double> idfScore = null;

    /**
     * Index set of text search.
     */
    private HashMap<Integer, TermLocation> textIndexs;

    /**
     * Index for wikipedia.
     */
    private HashMap<Integer, Integer> wikiIndexs;

    /**
     * Knowledge index.
     */
    private HashMap<Integer, KTermLocation> knowledgeIndexs;

    /**
     * Index of image.
     */
    private HashMap<Integer, TermLocation> imageIndexs;

    /**
     * Is in loading.
     */
    private boolean isLoadingText;

    /**
     * Is in loading image index.
     */
    private boolean isLoadingImage;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(SCallback.class.getName());

    /**
     * Load Index timestamp.
     */
    private long indexTimestamp = System.currentTimeMillis();

    /**
     * Initialization.
     */
    public void init() {

        // 1. Load IDF.
        loadIdf();

        // 2. Initialization.
        TextDatabae.init("Text_Index_Ready");
        ImageDatabase.init("Image_Index_Ready");
    }

    @Override
    public InternetPacket invoke(InternetPacket packet) {

        // 1. Test search.
        if (packet.getExecutType() == Exchange.ExecuteType.SEARCH.ordinal()) {
            String content = Exchange.getString(packet.getData());
            String[] keywords = com.iveely.framework.segment.Markov.getInstance().split(content);
            String result = queryWiki(content, keywords);
            result += queryKnowlege(content, keywords);
            result += queryText(content, keywords, 5);
            InternetPacket respPacket = new InternetPacket();
            respPacket.setData(Exchange.getBytes(result));
            respPacket.setExecutType(Exchange.ExecuteType.RESP_SEARCH.ordinal());
            return respPacket;
        } else if (packet.getExecutType() == Exchange.ExecuteType.IMAGE.ordinal()) {
            String content = Exchange.getString(packet.getData());
            String[] keywords = com.iveely.framework.segment.Markov.getInstance().split(content);
            InternetPacket respPacket = new InternetPacket();
            respPacket.setData(Exchange.getBytes(queryImage(content, keywords, 1)));
            respPacket.setExecutType(Exchange.ExecuteType.RESP_IMAGE.ordinal());
            return respPacket;
        } else if (packet.getExecutType() == Exchange.ExecuteType.ADDSCORE.ordinal()) {
            String[] content = Exchange.getString(packet.getData()).split("-");
            int pageId = Integer.parseInt(content[1]);
            String keyword = content[0];
            UserClick userClick = new UserClick();
            userClick.setPageId(pageId);
            userClick.setQuery(keyword);
            userClick.setTimestamp(System.currentTimeMillis());
            UserLogger.getInstance().addClickLog(userClick);
            InternetPacket respPacket = new InternetPacket();
            respPacket.setData(Exchange.getBytes("Add success."));
            respPacket.setExecutType(Exchange.ExecuteType.RESP_ADDSCORE.ordinal());
            return respPacket;
        } else if (packet.getExecutType() == Exchange.ExecuteType.SNAPSHOT.ordinal()) {
            String[] content = Exchange.getString(packet.getData()).split("-");
            int pageId = Integer.parseInt(content[1]);
            String keyword = content[0];
            InternetPacket respPacket = new InternetPacket();
            respPacket.setExecutType(Exchange.ExecuteType.RESP_SNAPSHOT.ordinal());
            HtmlPage page = TextDatabae.getInstance().getPage(pageId);
            if (page != null) {
                String snapshotText = page.getSnapshot(com.iveely.framework.segment.Markov.getInstance().split(keyword));
                respPacket.setData(Exchange.getBytes(snapshotText));
                return respPacket;
            } else {
                respPacket.setData(Exchange.getBytes("Not found page."));
                return respPacket;
            }
        }
        return InternetPacket.getUnknowPacket();
    }

    /**
     * Load IDF.
     *
     * @return
     */
    private boolean loadIdf() {
        idfScore = new TreeMap<>();
        List<String> idfs = Reader.readAllLine("Common/IDF.txt", "utf-8");
        if (idfs != null && idfs.size() > 0) {
            idfs.stream().map((idf) -> idf.split(" ")).filter((infor) -> (infor.length == 2)).filter((infor) -> (!idfScore.containsKey(infor[0].hashCode()))).forEach((infor) -> {
                try {
                    double weight = Double.parseDouble(infor[1]);
                    idfScore.put(infor[0].hashCode(), weight);
                } catch (NumberFormatException e) {
                }
            });
        }
        return true;
    }

    /**
     * Text search.
     *
     * @param userQuery
     * @param queries
     * @param retCount
     * @return
     */
    public String queryText(String userQuery, String[] queries, int retCount) {
        logger.info("DataService get query:" + userQuery);
        if (queries == null || textIndexs == null || isLoadingText) {
            return "NULL";
        }

        //1. Get the result set.
        logger.debug("1. Get pages' ids:" + userQuery);
        TreeSet<TermInverted> resultMap = new TreeSet<>();
        HashMap<Integer, TermInverted> singleIndex = new HashMap<>();
        List<String> terms = new ArrayList<>();
        boolean isHit = false;
        terms.addAll(Arrays.asList(queries));
        for (String querie : queries) {
            // Get children of term to get more information.
            String[] childTerms = com.iveely.framework.segment.Markov.getInstance().split(querie);
            for (String childTerm : childTerms) {
                if (!terms.contains(childTerm)) {
                    terms.add(childTerm);
                }
            }
        }
        int norSize = queries.length;
        for (int k = 0; k < terms.size(); k++) {
            int hashCode = terms.get(k).hashCode();
            logger.debug(terms.get(k) + ":" + hashCode);

            // Indexing system reads.
            TermLocation location = textIndexs.get(hashCode);
            if (location == null) {
                continue;
            }

            List<TermInverted> orgiList = TextDatabae.getInstance().getTermInverted(location, 50);
            if (orgiList != null && orgiList.size() > 0) {
                double score = Score.getIdfNormal();
                if (idfScore.containsKey(hashCode)) {
                    score = idfScore.get(hashCode);
                }
                for (TermInverted temp : orgiList) {
                    int pageId = temp.getPage();
                    if (singleIndex.containsKey(pageId)) {
                        TermInverted readyIndex = singleIndex.get(pageId);
                        if (k < norSize) {
                            readyIndex.setRank((readyIndex.getRank() + temp.getRank() * score) * Score.getRelative());
                        } else {
                            readyIndex.setRank((readyIndex.getRank() + temp.getRank() * score) * Score.getChildWordRelative() * Score.getChildWordRank());
                        }
                    } else {
                        if (k < norSize) {
                            isHit = true;
                            temp.setRank(temp.getRank() * score);
                        } else {
                            temp.setRank(temp.getRank() * score * Score.getChildWordRank());
                        }
                        singleIndex.put(pageId, temp);
                    }
                }
            }

            // Feedback score read.
            // List<IndexStruct> scoreList = ConfigDatabase.readTopIndex(String.valueOf(hashCode), 30);
            // if (scoreList != null && scoreList.size() > 0) {
            //     double score = 0.9;
            //     if (idfScore.containsKey(hashCode)) {
            //         score = idfScore.get(hashCode);
            //     }
            //     for (int i = 0; i < scoreList.size(); i++) {
            //         IndexStruct temp = scoreList.get(i);
            //         temp.setRank(temp.getRank() * score);
            //         resultMap.add(temp);
            //         }
            //  }
        }
        for (Map.Entry entry : singleIndex.entrySet()) {
            TermInverted ti = (TermInverted) entry.getValue();
            if (ti.getPage() < 0 || ti.getRank() == 0) {
                continue;
            }
            TermInverted temp = new TermInverted();
            temp.setPage(ti.getPage());
            temp.setRank(ti.getRank());
            temp.setTerm(ti.getTerm());
            resultMap.add(temp);
        }

        //2. Read the corresponding data.
        if (!isHit) {
            queries = terms.toArray(queries);
        }
        logger.debug("2. Get pages' data by id:" + userQuery);
        HashSet<Integer> resultPages = new HashSet<>();
        StringBuilder buffer = new StringBuilder();
        Iterator<TermInverted> it = resultMap.iterator();
        while (it.hasNext() && retCount > 0) {
            TermInverted docInfo = it.next();
            if (docInfo.getPage() < 0) {
                continue;
            }
            HtmlPage pageStruct;
            pageStruct = TextDatabae.getInstance().getPage(docInfo.getPage());
            if (pageStruct == null) {
                continue;
            }
            int titleHash = pageStruct.GetTilte().hashCode();
            int contentHash = pageStruct.GetContent().hashCode();
            if (resultPages.contains(titleHash) || resultPages.contains(contentHash)) {
                continue;
            }
            resultPages.add(titleHash);
            resultPages.add(contentHash);
            String abstr = pageStruct.simple(userQuery, queries, docInfo.getPage(), docInfo.getRank(), false);
            if (abstr != null) {
                buffer.append(abstr);
                buffer.append("[PAGERECORD]");
            }
            retCount--;
        }
        if (buffer.length() != 0) {
            logger.debug("Query text return query:" + userQuery);
            return buffer.toString();
        }
        logger.debug("Query empty return query:" + userQuery);
        return "Not found any pages.";
    }

    /**
     * Query image data.
     *
     * @param userQuery
     * @param queries
     * @param retCount
     * @return
     */
    public String queryImage(String userQuery, String[] queries, int retCount) {
        logger.info("Image search get query:" + userQuery);
        if (queries == null || imageIndexs == null || isLoadingImage) {
            return "NULL";
        }

        //1. 获取结果集合
        logger.debug("1. Get user query:" + userQuery);
        TreeSet<TermInverted> resultMap = new TreeSet<>();
        HashMap<Integer, TermInverted> singleIndex = new HashMap<>();
        List<String> terms = new ArrayList<>();
        boolean isHit = false;
        terms.addAll(Arrays.asList(queries));
        for (String querie : queries) {
            // Get children of term to get more information.
            String[] childTerms = com.iveely.framework.segment.Markov.getInstance().split(querie);
            for (String childTerm : childTerms) {
                if (!terms.contains(childTerm)) {
                    terms.add(childTerm);
                }
            }
        }
        int norSize = queries.length;
        for (int k = 0; k < terms.size(); k++) {
            int hashCode = terms.get(k).hashCode();
            logger.debug(terms.get(k) + ":" + hashCode);

            // Indexing system reads.
            TermLocation location = imageIndexs.get(hashCode);
            if (location == null) {
                continue;
            }

            List<TermInverted> orgiList = ImageDatabase.getInstance().getTermInverted(location, 50);
            if (orgiList != null && orgiList.size() > 0) {
                double score = Score.getIdfNormal();
                if (idfScore.containsKey(hashCode)) {
                    score = idfScore.get(hashCode);
                }
                for (TermInverted temp : orgiList) {
                    int pageId = temp.getPage();
                    if (singleIndex.containsKey(pageId)) {
                        TermInverted readyIndex = singleIndex.get(pageId);
                        if (k < norSize) {
                            readyIndex.setRank((readyIndex.getRank() + temp.getRank() * score) * Score.getRelative());
                        } else {
                            readyIndex.setRank((readyIndex.getRank() + temp.getRank() * score) * Score.getChildWordRelative() * Score.getChildWordRank());
                        }
                    } else {
                        if (k < norSize) {
                            isHit = true;
                            temp.setRank(temp.getRank() * score);
                        } else {
                            temp.setRank(temp.getRank() * score * Score.getChildWordRank());
                        }
                        singleIndex.put(pageId, temp);
                    }
                }
            }
        }
        for (Map.Entry entry : singleIndex.entrySet()) {
            TermInverted ti = (TermInverted) entry.getValue();
            if (ti.getPage() < 0 || ti.getRank() == 0) {
                continue;
            }
            TermInverted temp = new TermInverted();
            temp.setPage(ti.getPage());
            temp.setRank(ti.getRank());
            temp.setTerm(ti.getTerm());
            resultMap.add(temp);
        }

        // 2. Read the corresponding data.
        logger.debug("2. Get images' data by id:" + userQuery);
        HashSet<Integer> resultPages = new HashSet<>();
        StringBuilder buffer = new StringBuilder();
        Iterator<TermInverted> it = resultMap.iterator();
        while (it.hasNext() && retCount > 0) {
            TermInverted docInfo = it.next();
            if (docInfo.getPage() < 0) {
                continue;
            }
            Image image;
            image = ImageDatabase.getInstance().getImage(docInfo.getPage());
            if (image == null) {
                continue;
            }
            int titleHash = image.getUrl().hashCode();
            if (resultPages.contains(titleHash)) {
                continue;
            }
            resultPages.add(titleHash);
            String abstr = image.simple(docInfo.getRank(), userQuery);
            if (abstr != null) {
                buffer.append(abstr);
                buffer.append("[IMAGERECORD]");
            }
            retCount--;
        }
        if (buffer.length() != 0) {
            logger.debug("Query image return query:" + userQuery);
            return buffer.toString();
        }
        logger.debug("Query empty return query:" + userQuery);
        return "Not found any images.";
    }

    /**
     * Query wikipedia result.
     *
     * @param userQuery
     * @param queries
     * @return
     */
    public String queryWiki(String userQuery, String[] queries) {
        if (wikiIndexs != null && !isLoadingText) {
            if (wikiIndexs.containsKey(userQuery.hashCode())) {
                int id = wikiIndexs.get(userQuery.hashCode());
                Wikipedia wikipedia = TextDatabae.getInstance().getWiki(id);
                if (wikipedia != null) {
                    HtmlPage page = new HtmlPage();
                    page.setId(-1);
                    page.SetTitle(userQuery);
                    page.SetContent(wikipedia.getAbsArticle());
                    page.SetPublishDate("");
                    page.setCrawlDate(1);
                    page.setUrl(wikipedia.getUrl());
                    page.setCode("");
                    return page.simple(userQuery, queries, -1, Double.MAX_VALUE, true) + "[PAGERECORD]";
                }
            }
        }
        return "";
    }

    /**
     * Query knowledge.
     *
     * @param userQuery
     * @param queries
     * @return
     */
    public String queryKnowlege(String userQuery, String[] queries) {
        if (knowledgeIndexs != null && !isLoadingText && queries.length < 8) {

            // 1. Get indexs.
            List<KnowledgeIndex> indexs = new ArrayList<>();
            for (String query : queries) {
                KTermLocation location = knowledgeIndexs.get(query.hashCode());
                if (location != null) {
                    List<KnowledgeIndex> list = TextDatabae.getInstance().getKnowledgeIndexs(location.getStartPostion(), location.getEndPostion(), 10);
                    indexs.addAll(list);
                }
            }

            // 2. Get same knowledge..
            HashMap<Integer, List<KnowledgeIndex>> ids = new HashMap<>();
            indexs.stream().forEach((index) -> {
                if (!ids.containsKey(index.getKnowledgeId())) {
                    List<KnowledgeIndex> temp = new ArrayList<>();
                    temp.add(index);
                    ids.put(index.getKnowledgeId(), temp);
                } else {
                    ids.get(index.getKnowledgeId()).add(index);
                }
            });

            // 3. Get bettters.
            Iterator<Entry<Integer, List<KnowledgeIndex>>> iter = ids.entrySet().iterator();
            while (iter.hasNext()) {
                List<KnowledgeIndex> list = iter.next().getValue();
                if (list.size() > 1) {
                    HashSet<Integer> locats = new HashSet<>();
                    locats.add(1);
                    locats.add(2);
                    locats.add(3);
                    list.stream().forEach((knowledgeIndex) -> {
                        locats.remove(knowledgeIndex.getLocation());
                    });
                    int knowledgeId = list.get(0).getKnowledgeId();
                    KnowledgeBase knowledgeBase = TextDatabae.getInstance().getKnowledgeBase(knowledgeId);
                    if (locats.size() == 1) {
                        if (knowledgeBase != null) {
                            String result = "";
                            Integer[] lefts = new Integer[1];
                            lefts = locats.toArray(lefts);
                            if (lefts[0] == 1) {
                                result = knowledgeBase.getEntityA();
                            }
//                            if (lefts[0] == 2) {
//                                result = knowledgeBase.getEntityB();
//                            }
                            if (lefts[0] == 3) {
                                result = knowledgeBase.getRelation();
                            }
                            if (result.length() > 0) {
                                HtmlPage page = new HtmlPage();
                                page.setId(-2);
                                page.SetTitle(userQuery);
                                page.SetContent(result);
                                page.SetPublishDate("");
                                page.setCrawlDate(1);
                                page.setUrl("");
                                page.setCode("");
                                return page.simple(userQuery, queries, -2, Double.MAX_VALUE, true) + "[PAGERECORD]";
                            }
                        }
                    }
                    if (locats.isEmpty()) {
                        HtmlPage page = new HtmlPage();
                        page.setId(-2);
                        page.SetTitle(userQuery);
                        page.SetContent(knowledgeBase.getRelation());
                        page.SetPublishDate("");
                        page.setCrawlDate(1);
                        page.setUrl("");
                        page.setCode("");
                        return page.simple(userQuery, queries, -2, Double.MAX_VALUE, true) + "[PAGERECORD]";
                    }
                }
            }
        }
        return "";
    }

    /**
     * Load text index.
     */
    public void loadTextIndex() {
        isLoadingText = true;

        // 1. Load text search index.
        textIndexs = new HashMap<>();
        List<TermLocation> locations = TextDatabae.getInstance().getTermLocations();
        locations.stream().forEach((location) -> {
            int key = location.getTerm();
            if (!textIndexs.containsKey(key)) {
                textIndexs.put(key, location);
            }
        });

        // 2. Load wikipedia search index.
        wikiIndexs = new HashMap<>();
        List<WikipediaLocation> wikiLocations = TextDatabae.getInstance().getWikipediaLocations();
        wikiLocations.stream().forEach((location) -> {
            Integer termCode = location.getTerm().hashCode();
            if (!wikiIndexs.containsKey(termCode)) {
                wikiIndexs.put(termCode, location.getStoreId());
            }
        });

        // 3. Load knowledge base index.
        knowledgeIndexs = new HashMap<>();
        List<KTermLocation> knowledgeLocations = TextDatabae.getInstance().getKTermLocations();
        knowledgeLocations.stream().filter((location) -> (!knowledgeIndexs.containsKey(location.getTerm()))).forEach((location) -> {
            knowledgeIndexs.put(location.getTerm(), location);
        });
        logger.info("load text-index success.");
        isLoadingText = false;
    }

    /**
     * Load image index.
     */
    public void loadImageIndex() {
        isLoadingImage = true;
        imageIndexs = new HashMap<>();
        List<TermLocation> locations = ImageDatabase.getInstance().getTermLocations();
        locations.stream().forEach((location) -> {
            int key = location.getTerm();
            if (!imageIndexs.containsKey(key)) {
                imageIndexs.put(key, location);
            }
        });
        logger.info("load image-index success.");
        isLoadingImage = false;
    }

    @Override
    public void run() {
        int textLastHour = -1;
        int imageLastHour = -1;
        while (true) {
            try {
                indexTimestamp = System.currentTimeMillis();

                // 1.Check text data.
                logger.info("check service_text_data need to online...last text update:" + textLastHour);
                File textFile = new File("Service_Text_Data");
                if (textFile.exists()) {
                    long mins = (indexTimestamp - textFile.lastModified()) / 1000 / 60;
                    if (mins > 1 && Calendar.getInstance().get(Calendar.HOUR_OF_DAY) != textLastHour) {
                        logger.info("update service_text_data to online...");
                        isLoadingText = true;
                        String indexName = "Service_Text_" + indexTimestamp;
                        if (textFile.renameTo(new File(indexName))) {
                            TextDatabae.drop();
                            TextDatabae.rename(indexName);
                            loadTextIndex();
                        }
                        textLastHour = Calendar.getInstance().get(Calendar.HOUR_OF_DAY);
                    } else {
                        logger.info("Not update text as folder in writting...mins=" + mins + " last update time:" + textLastHour);
                    }
                }

                // 2. Check image data.
                logger.info("check service_image_data need to online...last image update:" + imageLastHour);
                File imageFile = new File("Service_Image_Data");
                if (imageFile.exists()) {
                    long mins = (indexTimestamp - imageFile.lastModified()) / 1000 / 60;
                    if (mins > 1 && Calendar.getInstance().get(Calendar.HOUR_OF_DAY) != imageLastHour) {
                        logger.info("update service_image_data to online...");
                        String indexName = "Service_Image_" + indexTimestamp;
                        if (imageFile.renameTo(new File(indexName))) {
                            ImageDatabase.drop();
                            ImageDatabase.rename(indexName);
                            loadImageIndex();
                        }
                        imageLastHour = Calendar.getInstance().get(Calendar.HOUR_OF_DAY);
                    } else {
                        logger.info("Not update image as folder in writting...mins=" + mins + " last update time:" + imageLastHour);
                    }
                }
                Thread.sleep(1000 * 60);
            } catch (InterruptedException ex) {
                logger.error(ex);
            }
        }
    }
}
