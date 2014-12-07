package iveely.search.image;

import com.iveely.framework.net.cache.Memory;
import com.iveely.framework.text.HashCounter;
import com.iveely.framework.text.UrlMisc;
import iveely.search.backstage.HtmlDownloader;
import iveely.search.store.TermInverted;
import iveely.search.store.Html2Article;
import iveely.search.store.Image;
import iveely.search.store.ImageDatabase;
import iveely.search.store.Status;
import iveely.search.store.Url;
import java.io.UnsupportedEncodingException;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.TreeMap;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.apache.log4j.Logger;

/**
 * Extractor of image search.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-23 10:30:25
 */
public class Extractor {

    /**
     * The urls which have visited
     */
    private HashSet<Integer> hasUrls = new HashSet<>();

    /**
     * Term invert index
     */
    private HashMap<Integer, List<TermInverted>> indexData;

    /**
     * Logger(log4j)
     */
    private Logger logger = Logger.getLogger(Extractor.class.getName());

    /**
     * The max visit size for each site.
     */
    private Integer maxVisitSize = 100;

    /**
     * The path of urls.
     */
    private String urlPath;

    /**
     * Whether this time has process url.
     */
    private boolean hasProcessUrl;

    /**
     * Initialization
     */
    public void init() {
        Random random = new Random();
        try {
            Thread.sleep(1000 * (random.nextInt(10) + 1));
        } catch (InterruptedException ex) {
            logger.warn("Radom time start failure.");
        }
        hasProcessUrl = false;
        this.urlPath = "Common/urls.txt";
        this.indexData = new HashMap<>();
        ImageDatabase.init("Image_Data");
        Memory.getInstance().initCache("Common/allClients.txt", false);
    }

    /**
     * Free memory manually.
     */
    private void freeMemoryManually() {
        if (indexData != null) {
            indexData.clear();
            indexData = null;
        }
        if (hasUrls != null) {
            hasUrls.clear();
            hasUrls = null;
        }
        logger = null;
    }

    /**
     * Call entry
     *
     * @param arg
     * @return
     */
    public boolean invoke(String arg) {

        // 1. Initialization.
        init();

        // 2 Check has new url.
        logger.info("check new url.");
        String preUrl = checkNewUrl();
        boolean result = false;
        if (preUrl != null && preUrl.length() > 5) {
            // 3.2 Process new url.
            result = processNewUrl(preUrl);
        } else {
            // 3.3 Update url.
            logger.info("Not found any new urls.");
        }
        // 4. Free meomory.
        freeMemoryManually();
        return result;
    }

    /**
     * Process new url.
     *
     * @param status
     * @return
     */
    private boolean processNewUrl(String preUrl) {

        // 1 Reget status as first load.
        int urlCount = 0;

        // 2. Get current url for visiting.
        logger.info("process new url,url=" + preUrl);
        String domain = UrlMisc.getDomain(preUrl);
        if (domain == null || domain.equals("") || !Memory.getInstance().get("Images_" + domain).equals("")) {
            logger.info("domin is null or memory has it.return.");
            return false;
        }
        Memory.getInstance().set("Images_" + domain, "0");

        // 3. Start crawl urls.
        List<String> imageUrls = new ArrayList<>();
        imageUrls.add(preUrl);
        int index = 0;
        while (maxVisitSize > 0 && index < imageUrls.size()) {

            // 3.0 Get url.
            String visitUrl = imageUrls.get(index);
            index++;

            // 3.1 When visit 100 records write index to avoid memory problem.
            logger.info("Url(" + maxVisitSize + "):" + visitUrl);
            if (maxVisitSize % 100 == 50) {
                writeIndex();
                indexData.clear();
            }
            // 3.2 Get html code.
            String content = HtmlDownloader.getHtmlContent(visitUrl, "UTF-8", null);
            if ("".equals(content) && content == null) {
                continue;
            }

            // 3.3 Index images.
            List<Image> images = Html2Article.getAllImages(0, visitUrl, content);
            if (images != null && images.size() > 0) {
                hasProcessUrl = true;
                int startId = ImageDatabase.getInstance().addImages(images) - images.size() + 1;
                for (Image image : images) {
                    image.setId(startId);
                    indexImage(image);
                    startId++;
                }
            }

            // 3.4 Get all effective urls in this page.
            if (urlCount < maxVisitSize) {
                List<String> urls = getChildUrls(visitUrl, 0, content);
                if (urls != null && urls.size() > 0) {
                    imageUrls.addAll(urls);
                    urlCount = imageUrls.size();
                }
            }
            maxVisitSize--;
        }

        // 4. Finish and exit.
        writeIndex();
        Memory.getInstance().set("Images_" + domain, "1");
        ImageDatabase.getInstance().dropTable(new Url());
        logger.info("Finish extract.");
        return hasProcessUrl;
    }

    /**
     * Update current data.
     *
     * @param status
     */
    private boolean updateData(Status status) {
        return false;
    }

    /**
     * First run or again cycle
     *
     * @param status
     * @return
     */
    private String checkNewUrl() {

        // 1. Get all urls.
        List<String> allUrls = com.iveely.framework.file.Reader.readAllLine(this.urlPath, "UTF-8");
        String preUrl = "";
        for (String allUrl : allUrls) {
            String[] urlText = allUrl.split(" ");
            if (!urlText[0].startsWith("http")) {
                preUrl = "http://" + urlText[0].replace("﻿", "");
            }
            // 1.1 Is it specify max visit count.(Image not need this)
//            if (urlText.length == 2) {
//                try {
//                    maxVisitSize = Integer.parseInt(urlText[1]);
//                } catch (Exception e) {
//                    e.printStackTrace();
//                }
//            }
            // 1.1 No slave process this url.
            String cacheDomain = UrlMisc.getDomain(preUrl);
            String domainStatus = Memory.getInstance().get("Images_" + cacheDomain);
            if (domainStatus != null && cacheDomain != null && domainStatus.equals("") && !"".equals(preUrl)) {
                break;
            }
        }

        // 2. Update url for next step to run.
        if (!preUrl.equals("")) {
            Url url = new Url();
            url.setTimestamp(-1);
            url.setUrl(preUrl);
            int urlId = ImageDatabase.getInstance().addUrl(url);
            if (urlId > -1) {
                return preUrl;
            }
            return "";
        } else {
            return "";
        }
    }

    /**
     * extract urls.
     *
     * @param fromUrl
     * @param htmlCode
     * @return
     */
    private List<String> extratUrls(String fromUrl, String htmlCode) {
        List<String> list = new ArrayList();
        try {
            int i = 0;
            final String regex = "<a.*?/a>";
            final Pattern pt = Pattern.compile(regex, Pattern.DOTALL);
            final Matcher mt = pt.matcher(htmlCode);
            while (mt.find()) {
                i++;
                // 1. Get link.
                final Matcher clickMatcher = Pattern.compile(">.*?</a>", Pattern.DOTALL).matcher(mt.group());
                String clickTitle = "";
                while (clickMatcher.find()) {
                    clickTitle = clickMatcher.group().replaceAll(">|</a>", "");
                }
                if (clickTitle.length() < 2) {
                    continue;
                }

                // 2. Get url.
                final Matcher urlMatcher = Pattern.compile("href=.*?>", Pattern.DOTALL).matcher(mt.group());
                String childUrl = "";
                while (urlMatcher.find()) {
                    childUrl = urlMatcher.group();
                    String urlCom = childUrl.split(" ")[0].replaceAll("href=|>", "");
                    if (urlCom.length() > 5) {
                        childUrl = joinUrl(fromUrl, urlCom);
                        if (childUrl.length() < 5) {
                            continue;
                        }
                        if (childUrl.endsWith("/")) {
                            childUrl = childUrl.substring(0, childUrl.length() - 1);
                        }
                    }
                }
                if (childUrl.length() > 5 && childUrl.toLowerCase().startsWith("http")) {
                    list.add(childUrl);
                }
            }
            return list;
        } catch (Exception e) {
            logger.error(e);
        }
        return list;
    }

    /**
     * URL 拼凑
     *
     * @param curl
     * @param file
     * @return
     */
    private String joinUrl(String curl, String file) {
        file = file.replace("\"", "").replace("'", "");
        if (file.toLowerCase().startsWith("http")) {
            return file;
        }
        if (file.toLowerCase().startsWith("javascript")) {
            return "";
        }
        URL url = null;
        String q = "";
        try {
            url = new URL(new URL(curl), file);
            q = url.toExternalForm();
        } catch (MalformedURLException e) {

        }
        url = null;
        if (q.contains("#")) {
            q = q.replaceAll("^(.+?)#.*?$", "$1");
        }
        return q.replace("/../", "/");
    }

    /**
     * Load urls which has visited
     */
    private void loadHasUrls() {
        List<Url> urls = ImageDatabase.getInstance().getUrls();
        urls.stream().map((url) -> url.getUrl().hashCode()).filter((code) -> (!hasUrls.contains(code))).forEach((code) -> {
            hasUrls.add(code);
        });
    }

    /**
     * Index page
     *
     * @param page
     */
    private void indexImage(Image image) {
        String alt = image.getAlt();
        String[] altWords = com.iveely.framework.segment.Markov.getInstance().split(alt);
        TreeMap<Integer, Double> altMap = HashCounter.statistic(altWords);
        if (altMap.size() > 0) {
            addIndex(altMap, image.getId());
        }
    }

    /**
     * Add index
     *
     * @param coefficient
     * @param totalCount
     * @param urlSocre
     * @param map
     */
    private void addIndex(TreeMap<Integer, Double> map, int artId) {
        Iterator<Integer> iterator = map.keySet().iterator();
        while (iterator.hasNext()) {
            int keywordHash = iterator.next();
            double rank = map.get(keywordHash) / map.size();
            TermInverted term = new TermInverted();
            term.setPage(artId);
            term.setRank(rank);
            term.setTerm(keywordHash);
            if (indexData.containsKey(keywordHash)) {
                List<TermInverted> list = indexData.get(keywordHash);
                list.add(term);
            } else {
                List<TermInverted> list = new ArrayList<>();
                list.add(term);
                indexData.put(keywordHash, list);
            }
        }
    }

    /**
     * Write index.
     */
    private void writeIndex() {
        List<TermInverted> list = new ArrayList<>();
        indexData.entrySet().stream().map((entry) -> {
            Object key = entry.getKey();
            return entry;
        }).map((entry) -> (List<TermInverted>) entry.getValue()).filter((val) -> (val != null && val.size() > 0)).forEach((val) -> {
            list.addAll(val);
        });
        if (list.size() > 0) {
            ImageDatabase.getInstance().addTerms(list);
        }
    }

    /**
     * Get all effective urls from html content.
     *
     * @param currentUrl
     * @param currentUrlId
     * @param content
     * @return
     */
    private List<String> getChildUrls(String currentUrl, Integer currentUrlId, String content) {
        List<String> allPageUrls = extratUrls(currentUrl, content);
        List<String> effectUrls = new ArrayList<>();
        String domain = UrlMisc.getDomain(currentUrl);
        allPageUrls.stream().filter((allPageUrl) -> (!hasUrls.contains(allPageUrl.hashCode()))).filter((allPageUrl) -> (domain.equals(UrlMisc.getDomain(allPageUrl)) && !effectUrls.contains(allPageUrl))).map((allPageUrl) -> allPageUrl).map((decodeUrl) -> {
            try {
                decodeUrl = java.net.URLDecoder.decode(decodeUrl, "utf-8");
            } catch (UnsupportedEncodingException e) {
                logger.error(decodeUrl + " encode error." + e);
            }
            return decodeUrl;
        }).map((decodeUrl) -> {
            hasUrls.add(decodeUrl.hashCode());
            return decodeUrl;
        }).forEach((decodeUrl) -> {
            effectUrls.add(decodeUrl);
        });
        if (allPageUrls.size() > 0) {
            return effectUrls;
        }
        return null;
    }
}
