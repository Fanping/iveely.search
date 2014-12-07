package iveely.search.backstage;

import com.iveely.framework.java.RefObject;
import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

/**
 * Html downloader.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-21 21:09:52
 */
public class HtmlDownloader {

    /**
     * Get web source code.
     *
     * @param url
     * @param charset
     * @param timestamp
     * @return
     */
    public static String getHtmlContent(String url, String charset, RefObject<Long> timestamp) {
        if (!url.toLowerCase().startsWith("http://")) {
            url = "http://" + url;
        }
        try {
            URL rUrl = new URL(url);
            return getHtmlContent(rUrl, charset, timestamp, 0);
        } catch (MalformedURLException e) {
        }
        return "";
    }

    /**
     * Get web source code.
     *
     * @param url
     * @param charset
     * @param timestamp
     * @param tryCount
     * @return
     */
    private static String getHtmlContent(URL url, String charset, RefObject<Long> timestamp, Integer tryCount) {
        if (tryCount > 3) {
            return null;
        }
        StringBuilder contentBuffer = new StringBuilder();
        int responseCode = -1;
        HttpURLConnection con = null;
        try {
            con = (HttpURLConnection) url.openConnection();
            con.setRequestProperty("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0;) like Gecko");
            if (timestamp != null) {
                con.setIfModifiedSince(timestamp.argvalue);
            }
            con.setConnectTimeout(5000);
            con.setReadTimeout(5000);
            responseCode = con.getResponseCode();
            if (responseCode == -1) {
                con.disconnect();
                return null;
            }
            if (responseCode >= 400) {
                con.disconnect();
                return null;
            }
            if (responseCode == 304) {
                return "";
            }
            String responseMsg = con.getContentType();
            if (!responseMsg.contains("text/html")) {
                con.disconnect();
                return null;
            }

            String charsetString = con.getContentEncoding();
            boolean isSureCharset = false;
            if (responseMsg.toUpperCase().contains("GB2312")) {
                charsetString = "GB2312";
                isSureCharset = true;
            } else if (responseMsg.toUpperCase().contains("GBK")) {
                charsetString = "GBK";
                isSureCharset = true;
            } else {
                charsetString = charset;
            }
            if (timestamp != null) {
                timestamp.argvalue = con.getIfModifiedSince();
            }
            try (InputStream inStr = con.getInputStream()) {
                InputStreamReader istreamReader = new InputStreamReader(inStr, charsetString);
                try (BufferedReader buffStr = new BufferedReader(istreamReader)) {
                    String str = null;
                    while ((str = buffStr.readLine()) != null) {
                        contentBuffer.append(str);
                    }
                    if (!isSureCharset) {
                        String metaCharset = getEncoding(contentBuffer.toString()).toUpperCase();
                        if (!metaCharset.equals(charsetString)) {
                            inStr.close();
                            String tryResult = getHtmlContent(url, metaCharset, timestamp, tryCount + 1);
                            if (tryResult != null) {
                                return tryResult;
                            }
                        }
                    }
                }
            }

        } catch (IOException e) {
            return null;
        } finally {
            if (con != null) {
                con.disconnect();
            }
        }
        return contentBuffer.toString();
    }

    /**
     * Get encoding of the page.
     *
     * @param html
     * @return
     */
    private static String getEncoding(String html) {
        int charsetStartIndex = html.toUpperCase().indexOf("CHARSET=");
        if (charsetStartIndex > 0) {
            int charsetEndIndex = html.indexOf(">", charsetStartIndex);
            if (charsetEndIndex - charsetStartIndex > 20) {
                charsetEndIndex = html.indexOf(" ", charsetStartIndex);
            }
            if (charsetEndIndex < html.length()) {
                String metaCharset = html.substring(charsetStartIndex + 8, charsetEndIndex).replace("\"", "").replace("/", "").replace("'", "").trim();
                return metaCharset;
            }
        }
        return "UTF-8";
    }
}
