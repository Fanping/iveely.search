/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.researcher;

import com.iveely.analyzer.common.HtmlDownloader;
import com.iveely.analyzer.data.WebSiteStorage;
import com.iveely.computing.api.FieldsDeclarer;
import com.iveely.computing.api.IInput;
import com.iveely.computing.api.IOutput;
import com.iveely.computing.api.StreamChannel;
import com.iveely.computing.api.TopologyBuilder;
import com.iveely.computing.api.TopologySubmitter;
import com.iveely.computing.api.Tuple;
import com.iveely.framework.net.Html2Article;
import com.iveely.framework.text.UrlMisc;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.HashMap;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.apache.log4j.Logger;

/**
 * Get information from all internet.
 *
 * @author 凡平
 */
public class WebSite {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(WebSite.class.getName());

    public static class UrlProvider extends IInput {

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
            declarer.declare(new String[]{"url", "htmlcode"}, new Integer[]{0});
        }

        @Override
        public void nextTuple() {
            boolean isGet = true;
            String val = getUrl();
            System.out.println("provider:" + val);
            if (!val.isEmpty()) {
                try {
                    String htmlCode = HtmlDownloader.getHtmlContent(val, "UTF-8");
                    if (htmlCode != null && !htmlCode.isEmpty()) {
                        this._channel.emit(val, htmlCode);
                    } else {
                        isGet = false;
                    }
                } catch (Exception e) {
                    logger.error(e);
                }

            } else {
                isGet = false;
            }
            if (!isGet) {
                try {
                    Thread.sleep(1000 * 5);
                } catch (InterruptedException ex) {
                    logger.error(ex);
                }
            }
        }

        @Override
        public void toOutput() {
            _channel.addOutputTo(new UrlExtractor());
            _channel.addOutputTo(new ContentExtractor());
        }

        @Override
        public void end(HashMap<String, Object> conf) {

        }

        /**
         * Get an url from iveely-db.
         *
         * @return
         */
        private String getUrl() {
            try {
                String strId = getPublicCache("website_id");
                if (strId.isEmpty()) {
                    setPublicCache("website_id", "0");
                    return "http://hao.360.cn";
                } else {
                    Integer intId = Integer.parseInt(strId);
                    setPublicCache("website_id", (intId + 1) + "");
                    String url = WebSiteStorage.getInstance().getUrl(intId);
                    return url;
                }
            } catch (Exception e) {
                logger.error(e);
            }
            return "";
        }
    }

    public static class UrlExtractor extends IOutput {

        /**
         * Urls cache to write.
         */
        private final List<Object[]> urls = new ArrayList<>();

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
            declarer.declare(new String[]{"url", "html"}, new Integer[]{0});
        }

        @Override
        public void execute(Tuple tuple) {
            String url = (String) tuple.get(0);
            String htmlCode = (String) tuple.get(1);
            if (htmlCode != null && !htmlCode.isEmpty()) {
                getMoreUrls(url, htmlCode);
            }
        }

        /**
         * Extract wikipedia information.
         *
         * @param htmlCode
         * @return
         */
        private void getMoreUrls(String url, String content) {
            List<Object[]> allUrls = extractUrls(url, content);
            if (allUrls != null && allUrls.size() > 0) {
                urls.addAll(allUrls);
            }
            if (urls.size() > 50) {
                WebSiteStorage.getInstance().addUrls(urls);
                urls.clear();
            }
        }

        /**
         * Extract urls.
         *
         * @param fromUrl
         * @param htmlCode
         * @return
         */
        private List<Object[]> extractUrls(String fromUrl, String htmlCode) {
            int i = 0;
            List<Object[]> list = new ArrayList();
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
                    list.add(new Object[]{childUrl.replace("\\", "")});
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
            URL url;
            String q = "";
            try {
                url = new URL(new URL(curl), file);
                q = url.toExternalForm();
            } catch (MalformedURLException e) {

            }
            if (q.contains("#")) {
                q = q.replaceAll("^(.+?)#.*?$", "$1");
            }
            return q.replace("/../", "/");
        }

        @Override
        public void toOutput() {

        }

        @Override
        public void end(HashMap<String, Object> conf) {
            if (urls.size() > 0) {
                WebSiteStorage.getInstance().addUrls(urls);
                urls.clear();
            }
        }

    }

    public static class ContentExtractor extends IOutput {

        private final List<Object[]> allDocuments = new ArrayList<>();

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
            declarer.declare(new String[]{"themeName", "url"}, new Integer[]{0});
        }

        @Override
        public void execute(Tuple tuple) {
            String url = (String) tuple.get(0);
            String code = (String) tuple.get(1);
            if (code == null || url == null) {
                return;
            }
            extractContent(url, code);
        }

        private void extractContent(String url, String code) {
            boolean isHost = UrlMisc.isHostUrl(url);
            boolean isDomain = UrlMisc.isDomainUrl(url);
            String crawDate = getTime();
            String logoUrl = "";
            String content = "";
            String title = Html2Article.getTitle(code, null);
            if (isHost) {
                content = Html2Article.getMetaDesc(code);
                logoUrl = Html2Article.getLogoUrl(url, code);
            }
            if (content.isEmpty()) {
                content = Html2Article.getContent(code).replace("\n", "");
            }
            String publishDate = Html2Article.getPublishDate(code);
            Object[] obj = new Object[8];
            obj[0] = title.replace("\\", "").replace("\"", "'").replace("	", "");
            obj[1] = url;
            obj[2] = content.replace("\\", "").replace("\"", "'").replace("	", "");
            obj[3] = isDomain;
            obj[4] = isHost;
            obj[5] = logoUrl.replace("\\", "").replace("\"", "'").replace("	", "");
            obj[6] = publishDate;
            obj[7] = crawDate;
            allDocuments.add(obj);
            if (allDocuments.size() > 10) {
                WebSiteStorage.getInstance().addPages(allDocuments);
                allDocuments.clear();
            }
        }

        private String getTime() {
            Calendar calendar = Calendar.getInstance();
            return calendar.getTime().toString();
        }

        @Override
        public void toOutput() {

        }

        @Override
        public void end(HashMap<String, Object> conf) {
            if (allDocuments.size() > 0) {
                WebSiteStorage.getInstance().addPages(allDocuments);
                allDocuments.clear();
            }
        }

    }

    public static void main(String[] args) {
        TopologyBuilder builder = new TopologyBuilder("WebSiteTopology");
        builder.setInput(new UrlProvider(), 1);
        builder.setSlave(1);
        builder.isLocalMode = true;
        builder.setOutput(new UrlExtractor(), 2);
        builder.setOutput(new ContentExtractor(), 4);
        TopologySubmitter.submit(builder, args);
    }
}
