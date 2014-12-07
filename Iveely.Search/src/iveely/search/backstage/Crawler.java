package iveely.search.backstage;

import iveely.search.store.TermInverted;
import com.iveely.framework.java.RefObject;
import com.iveely.framework.net.cache.Memory;
import com.iveely.framework.text.HashCounter;
import com.iveely.framework.text.UrlMisc;
import iveely.search.service.Score;
import iveely.search.store.TextDatabae;
import iveely.search.store.Html2Article;
import iveely.search.store.HtmlPage;
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
 * Crawler.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-21 21:05:23
 */
public class Crawler {

    /**
     * The urls which have visited
     */
    private HashSet<Integer> hasUrls;

    /**
     * Term invert index
     */
    private HashMap<Integer, List<TermInverted>> indexData;

    /**
     * Logger(log4j)
     */
    private final Logger logger = Logger.getLogger(Crawler.class.getName());

    /**
     * The max visit size for each site.
     */
    private Integer maxVisitSize = 50;

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
        hasUrls = new HashSet<>();
        TextDatabae.init("Text_Data");
        Memory.getInstance().initCache("Common/allClients.txt", false);
    }

    public void clean() {
        if (hasUrls != null) {
            hasUrls.clear();
        }
        if (indexData != null) {
            indexData.clear();
        }
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

        // 2. Get current status, restart or first start.
        Status status = TextDatabae.getInstance().getStatus();
        // 3. Whether first start.
        if (status != null) {
            //status.setPageId(0);
        }
        // 3.1 Check has new url.
        boolean hasNewUrl = checkNewUrl(status);
        if (hasNewUrl) {
            // 3.2 Process new url.
            return processNewUrl(status);
        } else {
            // 3.3 Update url.
            status = new Status();
            status.setPageId(0);
            return updateData(status);
        }

    }

    /**
     * Process new url.
     *
     * @param status
     * @return
     */
    private boolean processNewUrl(Status status) {

        // 1 Reget status as first load.
        int urlCount = 0;
        status = TextDatabae.getInstance().getStatus();
        if (status == null) {
            return false;
        }

        // 2. Get current url for visiting.
        int currentUrlId = status.getUrlId();
        Url visitUrl = TextDatabae.getInstance().getUrl(currentUrlId);
        String domain = UrlMisc.getDomain(visitUrl.getUrl());
        if (domain == null || domain.equals("") || !Memory.getInstance().get(domain).equals("")) {
            status.setUrlId(0);
            TextDatabae.getInstance().updateStatus(status);
            return false;
        }
        Memory.getInstance().set(domain, "0");

        // 3. Start crawl urls.
        logger.info("First url,url Id=" + currentUrlId);
        while (currentUrlId > -1 && visitUrl != null && maxVisitSize > 0 && !visitUrl.getUrl().equals("")) {
            // 3.1 When visit 100 records write index to avoid memory problem.
            logger.info("Url(" + maxVisitSize + "):" + visitUrl.getUrl());
            if (currentUrlId % 100 == 99) {
                writeIndex();
                indexData.clear();
            }
            // 3.2 Get html code.
            RefObject<Long> timestamp = new RefObject<>(visitUrl.getTimestamp());
            String content = getHtmlPage(visitUrl.getUrl(), timestamp, status);
            if (content != null && content.length() > 0) {
                // 3.3 Get all effective urls in this page.
                if (urlCount < maxVisitSize) {
                    Url[] urls = getChildUrls(visitUrl, currentUrlId, content);
                    if (urls != null && urls.length > 0) {
                        TextDatabae.getInstance().addUrls(urls);
                        urlCount += urls.length;
                    }
                }

            } else {
                logger.error("Content is null.");
            }

            // 3.4 Record this visit timestamp.(For server response not modify)
            if (timestamp.argvalue != -1) {
                visitUrl.setTimestamp(timestamp.argvalue);
                TextDatabae.getInstance().updateUrl(currentUrlId, visitUrl);
            }

            // 3.5 Get anthor url for visiting. 
            currentUrlId++;
            status.setUrlId(currentUrlId);
            TextDatabae.getInstance().updateStatus(status);
            visitUrl = TextDatabae.getInstance().getUrl(currentUrlId);
            maxVisitSize--;
        }

        // 4. Finish and exit.
        writeIndex();
        status.setUrlId(0);
        Memory.getInstance().set(domain, "1");
        TextDatabae.getInstance().updateStatus(status);
        TextDatabae.getInstance().dropTable(new Url());
        logger.info("exit.");
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
    private boolean checkNewUrl(Status status) {

        // 1. Get all urls.
        List<String> allUrls = com.iveely.framework.file.Reader.readAllLine(this.urlPath, "UTF-8");
        String preUrl = "";
        for (String allUrl : allUrls) {
            String[] urlText = allUrl.split(" ");
            if (!urlText[0].startsWith("http")) {
                preUrl = "http://" + urlText[0].replace("﻿", "");
            }
            // 1.1 No slave process this url.
            String cacheDomain = UrlMisc.getDomain(preUrl);
            String domainStatus = Memory.getInstance().get(cacheDomain);
            if (domainStatus != null && cacheDomain != null && domainStatus.equals("") && !"".equals(preUrl)) {

                // 1.2 Is it specify max visit count.
                if (urlText.length == 2) {
                    try {
                        maxVisitSize = Integer.parseInt(urlText[1]);
                    } catch (NumberFormatException e) {
                        logger.error(e);
                    }
                }
                break;
            }
        }

        // 2. Update url for next step to run.
        if (!preUrl.equals("")) {
            Url url = new Url();
            url.setTimestamp(-1);
            url.setUrl(preUrl);
            int urlId = TextDatabae.getInstance().addUrl(url);
            status = new Status();
            status.setPageId(-1);
            status.setUrlId(urlId);
            TextDatabae.getInstance().updateStatus(status);
            return true;
        } else {
            return false;
        }
    }

    /**
     * Get html source code
     *
     * @param url
     * @param timestamp
     */
    private String getHtmlPage(String url, RefObject<Long> timestamp, Status status) {

        // 1. Use donwloader get htlm code.
        String content = HtmlDownloader.getHtmlContent(url, "UTF-8", timestamp);
        if ("".equals(content) && content == null) {
            return "";
        }

        // 2. Build page object as structured data
        HtmlPage page = new HtmlPage();
        page.setCode(content);
        page.setUrl(url);
        page.setIsHost(UrlMisc.idHostUrl(url));
        page.setIsDomain(UrlMisc.isDomainUrl(url));
        page.setCrawlDate(System.currentTimeMillis());
        String imageUrl = " ";
        if (page.isIsHost()) {
            imageUrl = Html2Article.getLogoUrl(url, content);
            if (imageUrl == null || imageUrl.length() < 5) {
                imageUrl = " ";
            }
        }
        page.setLogoUrl(imageUrl);
        Html2Article.getArticle(page);
        int id = TextDatabae.getInstance().addPage(page);
        page.setId(id);
        status.setPageId(id);
        hasProcessUrl = true;

        // 3. Index page.
        if (page.GetContent() != null) {
            indexPage(page);
        }
        return content;
    }

    /**
     * Extract urls.
     *
     * @param fromUrl
     * @param htmlCode
     * @return
     */
    private List<String> extratUrls(String fromUrl, String htmlCode) {
        int i = 0;
        List<String> list = new ArrayList();
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
    }

    /**
     * URL join.
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
        List<Url> urls = TextDatabae.getInstance().getUrls();
        urls.stream().map((url) -> url.getUrl().hashCode()).filter((code) -> (!hasUrls.contains(code))).forEach((code) -> {
            hasUrls.add(code);
        });
    }

    /**
     * Index page
     *
     * @param page
     */
    private void indexPage(HtmlPage article) {
        // 1. Get url socre.
        double urlSocre = getUrlScore(article.getUrl());
        int base = 0;
        if (article.isIsDomain()) {
            base++;
        }
        if (article.isIsHost()) {
            base++;
        }
        urlSocre = urlSocre + base * 0.1;

        // 1. Index for title.
        String title = article.GetTilte().contains("-") ? article.GetTilte().substring(0, article.GetTilte().lastIndexOf("-")) : article.GetTilte();
        String[] titleWords = com.iveely.framework.segment.Markov.getInstance().split(title);
        TreeMap<Integer, Double> titleMap = HashCounter.statistic(titleWords);
        if (titleMap.size() > 0) {
            addIndex(Score.getTitleScore(), urlSocre, titleMap, article.getId());
        }

        // 2. Index for content.
        String[] contentWords = com.iveely.framework.segment.Markov.getInstance().split(article.GetContent());
        TreeMap<Integer, Double> contentMap = HashCounter.statistic(contentWords);
        if (contentMap.size() > 0) {
            addIndex(Score.getContentSocre(), urlSocre, contentMap, article.getId());
        }

        // 3. Index for url.
        if (article.isIsHost()) {
            String currentUrl = article.getUrl();
            String[] urlComs = currentUrl.split("\\.");
            String representUrl = "";
            if (urlComs.length == 2) {
                representUrl = urlComs[0];
            } else if (urlComs.length > 2) {
                representUrl = urlComs[1];
            } else {
                representUrl = "";
            }
            if (representUrl.length() > 0) {
                String[] urlWords = com.iveely.framework.segment.Markov.getInstance().split(representUrl);
                TreeMap<Integer, Double> urlMap = HashCounter.statistic(urlWords);
                if (urlMap.size() > 0) {
                    addIndex(Score.getUrlScore(), urlSocre, urlMap, article.getId());
                }
            }
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
    private void addIndex(double coefficient, double urlSocre, TreeMap<Integer, Double> map, int artId) {
        Iterator<Integer> iterator = map.keySet().iterator();
        while (iterator.hasNext()) {
            int keywordHash = iterator.next();
            double rank = map.get(keywordHash) / coefficient + map.get(keywordHash) / map.size() + urlSocre;
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
     * Get score of the url.
     *
     * @param url
     * @return
     */
    private double getUrlScore(String url) {
        double score = (10 - url.split("/").length * 1.0 - 3) + url.length() / 1000.0;
        score = score > 0 ? score * 0.01 : 0;
        if (UrlMisc.isDomainUrl(url)) {
            score += 0.05;
        }
        if (url.contains(".gov") || url.contains(".edu")) {
            score += 0.1;
        }
        return score;
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
            TextDatabae.getInstance().addTerms(list);
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
    private Url[] getChildUrls(Url currentUrl, Integer currentUrlId, String content) {
        List<String> allPageUrls = extratUrls(currentUrl.getUrl(), content);
        List<String> effectUrls = new ArrayList<>();
        String domain = UrlMisc.getDomain(currentUrl.getUrl());
        for (String allPageUrl : allPageUrls) {
            if (!hasUrls.contains(allPageUrl.hashCode())) {
                if (domain.equals(UrlMisc.getDomain(allPageUrl)) && !effectUrls.contains(allPageUrl)) {
                    String decodeUrl = allPageUrl;
                    try {
                        decodeUrl = java.net.URLDecoder.decode(decodeUrl, "utf-8");
                    } catch (UnsupportedEncodingException e) {
                        logger.error(decodeUrl + " encode error." + e);
                    }
                    hasUrls.add(decodeUrl.hashCode());
                    effectUrls.add(decodeUrl);
                }
            }
        }
        if (allPageUrls.size() > 0) {
            String[] newUrls = new String[effectUrls.size()];
            newUrls = effectUrls.toArray(newUrls);
            Url[] urls = new Url[effectUrls.size()];
            for (int k = 0; k < effectUrls.size(); k++) {
                Url tUrl = new Url();
                tUrl.setParentUrl(currentUrlId);
                tUrl.setUrl(newUrls[k]);
                urls[k] = tUrl;
            }
            return urls;
        }
        return null;
    }
}
