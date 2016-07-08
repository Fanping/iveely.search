package com.iveely.crawler.entity;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class Article {

  private String title;

  private String content;

  private Long crawlTimestamp;

  public Article(final String title, final String content) {
    this.title = title;
    this.content = content;
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
}
