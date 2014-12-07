package iveely.search.store;

import com.iveely.framework.database.type.ShortString;
import iveely.search.service.SlaveService;
import java.util.ArrayList;
import java.util.List;

/**
 * Page document conversion is completed.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-21 21:18:47
 */
public class HtmlPage {

    public HtmlPage() {
    }

    /**
     * Page numbering.
     */
    private int id;

    /**
     * @return the id
     */
    public int getId() {
        return id;
    }

    /**
     * @param id the id to set
     */
    public void setId(int id) {
        this.id = id;
    }

    /**
     * Source code.
     */
    private String code;

    /**
     * @return the code
     */
    public String getCode() {
        return code;
    }

    /**
     * @param code the code to set
     */
    public void setCode(String code) {
        this.code = code;
    }

    /**
     * Article text.
     */
    private String content;

    public void SetContent(String content) {
        this.content = content;
    }

    public String GetContent() {
        return content;
    }

    /**
     * Article title.
     */
    private String title;

    public void SetTitle(String tilte) {
        title = tilte;
    }

    public String GetTilte() {
        return title;
    }

    /**
     * URL access.
     */
    private ShortString url;

    /**
     * @return the url
     */
    public String getUrl() {
        if (this.url != null) {
            return url.getValue();
        } else {
            return "";
        }
    }

    /**
     * @param url the url to set
     */
    public void setUrl(String url) {
        try {
            this.url = new ShortString(url);
        } catch (Exception e) {
        }
    }

    /**
     * Is domain.
     */
    private boolean isDomain;

    /**
     * @return the isDomain
     */
    public boolean isIsDomain() {
        return isDomain;
    }

    /**
     * @param isDomain the isDomain to set
     */
    public void setIsDomain(boolean isDomain) {
        this.isDomain = isDomain;
    }

    /**
     * Is host.
     */
    private boolean isHost;

    /**
     * Publish date.
     */
    private ShortString publishDate;

    public void SetPublishDate(String publishDate) {
        try {
            this.publishDate = new ShortString(publishDate);
        } catch (Exception ex) {

        }
    }

    public String GetPublishDate() {
        if (publishDate != null) {
            return publishDate.getValue();
        }
        return "";
    }

    /**
     * Crawl date.
     */
    private long crawlDate;

    /**
     * Url of logo
     */
    private ShortString logoUrl;

    public String simple(String query, String[] keywords, Integer recordId, Double rank, boolean isWiki) {
        String simpleTitle = (title.length() > 28 ? title.substring(0, 27) + "..." : title);
        String colorTitle = isWiki ? simpleTitle : changeColor(keywords, simpleTitle).replace("<!", "").trim();
        String colorContent = isWiki ? content : changeColor(keywords, isWiki ? content : checkContent(content.replace("  ", ""), keywords));
        if ((!isWiki) && (colorTitle.length() < 2 || colorContent.length() < 2)) {
            return null;
        }
        return buildTextResult(query, recordId, colorTitle, colorContent, url.getValue(), publishDate.getValue(), "", rank);
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

    /**
     * Build text search result.
     *
     * @param query
     * @param id
     * @param title
     * @param content
     * @param url
     * @param date
     * @param classify
     * @param rank
     * @return
     */
    private String buildTextResult(String query, Integer id, String title, String content, String url, String date, String classify, double rank) {
        StringBuilder buffer = new StringBuilder();
        buffer.append("[WEIGHT]:");
        buffer.append(rank);
        buffer.append("\n");
        buffer.append("[PAGEID]:");
        buffer.append(id);
        buffer.append("\n");
        buffer.append("[TITLE]:");
        buffer.append(title);
        buffer.append("\n");
        buffer.append("[URL]:");
        buffer.append(url);
        buffer.append("\n");
        buffer.append("[IMAGEURL]:");
        buffer.append(getLogoUrl());
        buffer.append("\n");
        buffer.append("[ABSTRACT]:");
        buffer.append(content);
        buffer.append("\n");
        buffer.append("[DATE]:");
        buffer.append(GetPublishDate());
        buffer.append("\n");
        buffer.append("[CRAWLDATE]:");
        buffer.append(getCrawlDate());
        buffer.append("\n");
        buffer.append("[QUERY]:");
        buffer.append(query);
        buffer.append("\n");
        buffer.append("[FROMSERVER]:");
        buffer.append(SlaveService.getFlag());
        buffer.append("\n");
        return buffer.toString();
    }

    /**
     * Check content length.
     *
     * @param content
     * @param keys
     * @return
     */
    public String checkContent(String content, String[] keys) {
        //Logger.debug(content);
        int abstractCount = 100;
        try {
            // 1. Determine the number of summary.
            if (content.length() < abstractCount) {
                return content;
            }

            // 2. Determine the number of blocks summary.
            int contentSize = content.length();
            int index = 0;
            List<Integer> list = new ArrayList<>();
            for (String key : keys) {
                index = content.indexOf(key, index);
                if (index < 0) {
                    continue;
                }
                list.add(index);
            }

            // 3. Up into three sections.
            if (list.size() == 1) {
                int place = list.get(0);
                if (contentSize - place < abstractCount) {
                    int start = place;
                    int end = place + 1;
                    while (end - start < abstractCount) {
                        if (start > 0) {
                            start--;
                        }
                        if (end < contentSize) {
                            end++;
                        }
                    }
                    return content.substring(start, end);
                } else if (place < abstractCount) {
                    return content.substring(0, abstractCount);
                } else {
                    int start = place - abstractCount / 2 - 1;
                    int end = place + abstractCount / 2;
                    if (end > contentSize - 1) {
                        end = contentSize - 1;
                    }
                    return content.substring(start, end);
                }
            } else if (list.size() > 1) {
                int placeA = list.get(0);
                int placeB = list.get(1);
                if (placeB - placeA < abstractCount) {
                    int start = placeA;
                    int end = placeB + 1;
                    while (end - start < abstractCount) {
                        if (start > 0) {
                            start--;
                        }
                        if (end < contentSize) {
                            end++;
                        }
                    }
                    if (end > contentSize - 1) {
                        end = contentSize - 1;
                    }
                    return content.substring(start, end);
                } else {
                    String bufferA = "";
                    if (placeA - abstractCount / 2 < 0) {
                        bufferA = content.substring(0, abstractCount / 2);
                    } else {
                        bufferA = content.substring(abstractCount / 2, abstractCount);
                    }
                    String bufferB = "";
                    if (contentSize - placeB < abstractCount / 2) {
                        int start = placeB + abstractCount / 2 - 1;
                        if (start > contentSize - 1) {
                            start = placeB;
                        }
                        bufferB = content.substring(start, contentSize - 1);
                    } else {
                        bufferB = content.substring(placeB - abstractCount / 4 - 1, placeB + abstractCount / 4);
                    }
                    return bufferA + bufferB;
                }
            } else {
                return content.substring(0, abstractCount) + "...";
            }
        } catch (Exception e) {
        }
        return content.substring(0, abstractCount) + "...";
    }

    @Override
    public String toString() {
        return title + "\n" + url + "\n" + content;
    }

    /**
     * Get snapshot.
     *
     * @param keys
     * @return
     */
    public String getSnapshot(String[] keys) {
        StringBuilder buffer = new StringBuilder();
        buffer.append("[TITLE]:");
        buffer.append(changeColor(keys, title));
        buffer.append("[URL]:");
        buffer.append(url);
        buffer.append("[DATE]:");
        buffer.append(GetPublishDate());
        buffer.append("[PUBLISH]:");
        buffer.append(changeColor(keys, code.replace(" ", "&nbsp;")));
        return buffer.toString().replace("\n", "<br/>");
    }

    /**
     * @return the crawlDate
     */
    public long getCrawlDate() {
        return crawlDate;
    }

    /**
     * @param crawlDate the crawlDate to set
     */
    public void setCrawlDate(long crawlDate) {
        this.crawlDate = crawlDate;
    }

    /**
     * @return the isHost
     */
    public boolean isIsHost() {
        return isHost;
    }

    /**
     * @param isHost the isHost to set
     */
    public void setIsHost(boolean isHost) {
        this.isHost = isHost;
    }

    /**
     * @return the logoUrl
     */
    public String getLogoUrl() {
        if (this.logoUrl != null) {
            return this.logoUrl.getValue();
        }
        return " ";
    }

    /**
     * @param logoUrl the logoUrl to set
     */
    public void setLogoUrl(String logoUrl) {
        try {
            this.logoUrl = new ShortString(logoUrl);
        } catch (Exception ex) {
            this.logoUrl = ShortString.getDefaultShortString();
        }
    }
}
