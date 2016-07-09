package com.iveely.crawler.entity;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class Article {

  private final String title;

  private final String content;

  private final String url;

  private Long crawlTimestamp;

  public Article(final String title, final String content,final String url) {
    this.title = title;
    this.content = content;
    this.url = url;
    this.crawlTimestamp = System.currentTimeMillis();
  }

  public String getTitle() {
    return title;
  }

  public String getContent() {
    return content;
  }

  public Long getCrawlTimestamp() {
    return crawlTimestamp;
  }

  public String getUrl() {
    return url;
  }
}
