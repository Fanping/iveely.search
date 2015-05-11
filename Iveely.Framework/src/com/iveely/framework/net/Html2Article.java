/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.framework.net;

import com.iveely.framework.java.RefObject;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.apache.log4j.Logger;

/**
 * Turn the page source code documentation.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-25 22:24:51
 */
public class Html2Article {

    /**
     * Regex filters: regular expressions, was asked to replace text.
     */
    private static final String[][] filters
            = {
                new String[]{"(?is)<script.*?>.*?</script>", ""},
                new String[]{"(?is)<style.*?>.*?</style>", ""},
                new String[]{"(?is)<!--.*?-->", ""},
                new String[]{"(?is)</a>", "</a>\n"}
            };

    /**
     * Append mode.
     */
    private static boolean appendMode = false;

    /**
     * After whether to use append mode, the default is false to use append
     * mode, all text will meet the filter criteria extracted.
     *
     * @return
     */
    public static boolean getAppendMode() {
        return appendMode;
    }

    /**
     * Set append mode.
     *
     * @param value
     */
    public static void setAppendMode(boolean value) {
        appendMode = value;
    }

    /**
     * In depth analysis of the line, the default is 6.
     */
    private static int depth = 6;

    /**
     * Get depth.
     *
     * @return
     */
    public static int getDepth() {
        return depth;
    }

    /**
     * Set depth.
     *
     * @param value
     */
    public static void setDepth(int value) {
        depth = value;
    }

    /**
     * Limit the number of characters, when the number reaches a limited number
     * of text analysis is considered 180 characters into the text content of
     * the default number.
     */
    private static int limitCount = 180;

    /**
     * Get limit count.
     *
     * @return
     */
    public static int getLimitCount() {
        return limitCount;
    }

    /**
     * Set limit count.
     *
     * @param value
     */
    public static void setLimitCount(int value) {
        limitCount = value;
    }

    /**
     * Determining the body of the article head, look upward, consecutive empty
     * lines reach headEmptyLines, then stop searching.
     */
    private static final int headEmptyLines = 2;

    /**
     * Used to determine the number of characters in the article ends.
     */
    private static final int endLimitCharCount = 20;

    /**
     * Logger(log4j)
     */
    private final static Logger logger = Logger.getLogger(Html2Article.class.getName());

    /**
     * Get information from a given text Html original text.
     *
     * @param code
     * @return
     */
    public static String getContent(String code) {
        if (code == null) {
            return "";
        }
        if (stringNumbers(0, code, "\n") < 10) {
            code = code.replace(">", ">\n").toLowerCase();
        }
        String body;
        String bodyFilter = "<body.*?</body>";
        Pattern pattern = Pattern.compile(bodyFilter, Pattern.DOTALL);
        Matcher matcher = pattern.matcher(code);
        StringBuilder sb = new StringBuilder();
        if (matcher.find()) {
            sb.append(matcher.group());
        }
        body = sb.toString();
        for (String[] filter : filters) {
            body = body.replaceAll(filter[0], filter[1]);
        }

        String content = null;
        String contentWithTags = null;
        RefObject<String> tempRef_content = new RefObject<>(content);
        RefObject<String> tempRef_contentWithTags = new RefObject<>(contentWithTags);
        getContent(body, tempRef_content, tempRef_contentWithTags);
        content = tempRef_content.argvalue.trim();
        if (content.length() < 20) {
            content = code.toLowerCase().replaceAll("<script(?:[^<]++|<(?!/script>))*+</script>", "").replaceAll("<style(?:[^<]++|<(?!/style>))*+</style>", "").replaceAll("<.*?>", "").trim();
        } else {
            content = content.toLowerCase().replaceAll("<script(?:[^<]++|<(?!/script>))*+</script>", "").replaceAll("<style(?:[^<]++|<(?!/style>))*+</style>", "").replaceAll("<.*?>", "").trim();
        }
        return content;
    }

    /**
     * Get meta description.
     *
     * @param code
     * @return
     */
    public static String getMetaDesc(String code) {
        String patternVal = "<meta.*?>";
        Pattern pattern = Pattern.compile(patternVal, Pattern.DOTALL);
        Matcher matcher = pattern.matcher(code);
        while (matcher.find()) {
            String temp = matcher.group();
            if (temp.contains("description")) {
                temp = temp.replace("content=", "").replace("<meta", "").replace("\"", "").replace("/>", "").trim().replace("name=description", "").replace(">", "");
                return temp;
            }
        }
        return "";
    }

    /**
     * Formatting tags, excluding matching label carriage.
     *
     * @param match
     * @return
     */
    private static String formatTag(Matcher match) {
        StringBuilder sb = new StringBuilder();
        for (char ch : match.group(0).toCharArray()) {
            if (ch == '\r' || ch == '\n') {
                continue;
            }
            sb.append(ch);
        }
        return sb.toString();
    }

    /**
     * Get title.
     *
     * @param html
     * @param backupTitle
     * @return
     */
    public static String getTitle(String html, String backupTitle) {
        String titleFilter = "<title>[\\s\\S]*?</title>";
        String h1Filter = "<h1.*?>.*?</h1>";
        String clearFilter = "<.*?>";
        String title = "";
        Pattern pattern = Pattern.compile(titleFilter, Pattern.DOTALL);
        Matcher matcher = pattern.matcher(html.toLowerCase());
        if (matcher.find()) {
            title = matcher.group().replaceAll(clearFilter, "").trim();
        }
        pattern = Pattern.compile(h1Filter, Pattern.DOTALL);
        matcher = pattern.matcher(html);
        if (matcher.find()) {
            String h1 = matcher.group(0).replace(clearFilter, "");
            if (!(h1 == null || h1.equals("")) && title.startsWith(h1)) {
                title = h1.split("-| ")[0].trim();
            }
        }
        return title;
    }

    /**
     * get publish date.
     *
     * @param html
     * @return
     */
    public static String getPublishDate(String html) {
        Pattern pattern = Pattern.compile("(?is)<.*?>", Pattern.DOTALL);
        Matcher matcher = pattern.matcher(html);
        String text = matcher.replaceAll("");
        pattern = Pattern.compile("((\\d{4}|\\d{2})(\\-|\\/)\\d{1,2}\\3\\d{1,2})(\\s?\\d{2}:\\d{2})?|(\\d{4}年\\d{1,2}月\\d{1,2}日)(\\s?\\d{2}:\\d{2})?", Pattern.DOTALL);
        matcher = pattern.matcher(text);
        if (matcher.find()) {
            try {
                String dateStr = "";
                for (int i = 0; i < matcher.groupCount(); i++) {
                    dateStr = matcher.group(i);
                    if (!(dateStr == null || dateStr.equals(""))) {
                        break;
                    }
                }
                if (dateStr.contains("年")) {
                    StringBuilder sb = new StringBuilder();
                    for (char ch : dateStr.toCharArray()) {
                        if (ch == '年' || ch == '月') {
                            sb.append("/");
                            continue;
                        }
                        if (ch == '日') {
                            sb.append(' ');
                            continue;
                        }
                        sb.append(ch);
                    }
                    dateStr = sb.toString();
                }
                return dateStr;
            } catch (RuntimeException ex) {
                System.out.println(ex);
            }
        }
        return "";
    }

    /**
     * Analysis of the text content from the body text of the label.
     *
     * @param bodyText
     * @param content
     * @param contentWithTags
     */
    private static void getContent(String bodyText, RefObject<String> content, RefObject<String> contentWithTags) {
        String[] orgLines;
        String[] lines;
        orgLines = bodyText.split("[\\n]", -1);
        lines = new String[orgLines.length];
        for (int i = 0; i < orgLines.length; i++) {
            String lineInfo = orgLines[i];
            Pattern pattern = Pattern.compile("(?is)</p>|<br.*?/>|</H1>|</H2>|</H3>");
            Matcher matcher = pattern.matcher(lineInfo);
            lineInfo = matcher.replaceAll("[crlf]");
            pattern = Pattern.compile("(?is)<.*?>");
            matcher = pattern.matcher(lineInfo);
            lines[i] = matcher.replaceAll("");
        }
        if (lines.length > 1000) {
            StringBuilder contentBuffer = new StringBuilder();
            for (String line : lines) {
                String temp = line.trim();
                if (temp.length() > 0) {
                    contentBuffer.append(temp).append(" ");
                }
            }
            content.argvalue = contentBuffer.toString().replace("[crlf]", "\n").replace(" 　　", "\n").replace("&nbsp;", "\n");
            return;
        }
        StringBuilder sb = new StringBuilder();
        StringBuilder orgSb = new StringBuilder();
        int preTextLen = 0;
        int startPos = -1;
        for (int i = 0; i < lines.length - depth; i++) {
            int len = 0;
            for (int j = 0; j < depth; j++) {
                len += lines[i + j].length();
            }
            if (startPos == -1) {
                if (preTextLen > limitCount && len > 0) {
                    int emptyCount = 0;
                    for (int j = i - 1; j > 0; j--) {
                        if (lines[j] == null || lines[j].equals("")) {
                            emptyCount++;
                        } else {
                            emptyCount = 0;
                        }
                        if (emptyCount == headEmptyLines) {
                            startPos = j + headEmptyLines;
                            break;
                        }
                    }
                    if (startPos == -1) {
                        startPos = i;
                    }
                    for (int j = startPos; j <= i; j++) {
                        sb.append(lines[j]);
                        orgSb.append(orgLines[j]);
                    }
                }
            } else {
                if (len <= endLimitCharCount && preTextLen < endLimitCharCount) {
                    if (!appendMode) {
                        break;
                    }
                    startPos = -1;
                }
                sb.append(lines[i]);
                orgSb.append(orgLines[i]);
            }
            preTextLen = len;
        }
        String result = sb.toString();
        content.argvalue = result.replace("[crlf]", "\n").replace(" 　　", "\n").replace("&nbsp;", "\n");
        contentWithTags.argvalue = orgSb.toString();
    }

    private static int stringNumbers(int defaultCount, String source, String childString) {
        if (!source.contains(childString)) {
            return defaultCount + 0;
        } else if (source.contains(childString)) {
            defaultCount++;
            return stringNumbers(defaultCount, source.substring(source.indexOf(childString) + childString.length()), childString);
        }
        return defaultCount;
    }

    /**
     * Get logo's url.
     *
     * @param fromUrl
     * @param htmlCode
     * @return
     */
    public static String getLogoUrl(String fromUrl, String htmlCode) {
        final String regex = "<link.*?>";
        String url = "";
        if (htmlCode == null) {
            return url;
        }
        final Pattern pt = Pattern.compile(regex, Pattern.DOTALL);
        final Matcher mt = pt.matcher(htmlCode);
        while (mt.find()) {
            final Matcher urlMatcher = Pattern.compile("href=.*?>", Pattern.DOTALL).matcher(mt.group());
            String childUrl;
            while (urlMatcher.find()) {
                childUrl = urlMatcher.group();
                if (childUrl.toLowerCase().contains(".ico")) {
                    String[] urlComs = childUrl.split(" ");
                    String urlCom = urlComs[0].replaceAll("href=|>", "");
                    if (urlCom.length() > 5) {
                        url = joinUrl(fromUrl, urlCom);
                        if (url.endsWith("/")) {
                            url = url.substring(0, url.length() - 1);
                        }
                        break;
                    }
                }
            }
        }
        if (url.length() < 5) {
            url = "";
        }
        return url;
    }

    /*
     Join url.
     */
    private static String joinUrl(String curl, String file) {
        file = file.replace("\"", "");
        if (file.toLowerCase().startsWith("http")) {
            return file;
        }
        URL url;
        String q = "";
        try {
            url = new URL(new URL(curl), file);
            q = url.toExternalForm();
        } catch (MalformedURLException e) {
            logger.error(e);
        }
        if (q.contains("#")) {
            q = q.replaceAll("^(.+?)#.*?$", "$1");
        }
        return q.replace("/../", "/");
    }
}
