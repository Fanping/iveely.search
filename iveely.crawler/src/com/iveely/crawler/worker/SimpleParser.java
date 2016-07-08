package com.iveely.crawler.worker;

import com.iveely.crawler.entity.Article;
import com.iveely.crawler.entity.WebUrl;

import org.jsoup.nodes.Document;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class SimpleParser {

  public boolean shouldVisit(final WebUrl refer, final WebUrl webUrl) {
    return true;
  }

  public Article get(final Document document) {
    return new Article(document.title(),
        document.body().text().replace(" ", ""));
  }
}
