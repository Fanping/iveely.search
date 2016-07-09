package com.iveely.crawler.worker;

import com.iveely.crawler.common.WildcardMatcher;
import com.iveely.crawler.config.Seed;
import com.iveely.crawler.entity.WebUrl;

import org.apache.commons.lang3.SerializationUtils;
import org.apache.commons.lang3.StringUtils;
import org.jsoup.Jsoup;
import org.jsoup.nodes.Document;
import org.jsoup.select.Elements;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

/**
 * @author Fanping liu (liufanping@iveely.com)
 */
public class SeedExecutor implements Runnable {

  private static Logger logger = LoggerFactory.getLogger(SeedExecutor.class);

  private final Seed seed;

  private List<WebUrl> queued;

  private String userAgent;

  private BloomFilter<String> filter;

  private SimpleParser parser;

  private File filterFile;

  public SeedExecutor(final Seed seed, final SimpleParser parser) {
    this.seed = seed;
    this.queued = new LinkedList<>();
    this.filter = new BloomFilter<String>(0.05, 1000);
    this.parser = parser;
    this.userAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 " +
        "(KHTML, like Gecko) Chrome/47.0.2526.106 Safari/537.36";
    this.filterFile = new File("conf/filter/" + this.seed.getName() + ".bin");
  }

  @Override
  public void run() {
    // 1. Init.
    Thread.currentThread().setName("Thread_" + this.seed.getName());
    if (!StringUtils.isBlank(this.seed.getUserAgent())) {
      this.userAgent = this.seed.getUserAgent();
    }
    if (this.filterFile.exists()) {
      try {
        this.filter = SerializationUtils.deserialize(new FileInputStream(this
            .filterFile));
      } catch (Exception ex) {
        this.filter = new BloomFilter<>(0.05, 1000);
        logger.error("Error happen when deserialize bloom filter", ex);
      }

    }

    // 2.Visit seed.
    WebUrl seedUrl = new WebUrl(1, this.seed.getUrl());
    queued.add(seedUrl);
    while (!queued.isEmpty()) {
      WebUrl webUrl = queued.remove(0);
      logger.info("Visit web url =>" + webUrl.getUrl() + " (left:" + queued
          .size()
          + ")");
      try {
        Document document = getDocument(webUrl);
        if (document == null) {
          continue;
        }
        List<WebUrl> urls = getUrls(webUrl, document);
        if (urls != null && !urls.isEmpty()) {
          queued.addAll(urls);
        }
        IndexRequest.post(parser.get(document, webUrl.getUrl()));
      } catch (Exception ex) {
        logger.error("Crawl url:" + webUrl.getUrl() + " failed.", ex);
      }
    }

    // 3. Data persistence.
    try {
      if (this.filterFile.exists()) {
        this.filterFile.delete();
      }
      this.filterFile.createNewFile();
      SerializationUtils.serialize(this.filter, new
          FileOutputStream(this.filterFile));
    } catch (Exception ex) {
      logger.error("Data persistence error happen.", ex);
    }

    Worker.remove(this.seed.getName());
    logger.info("Finish crawl seed:" + this.seed.getName());
  }

  protected Document getDocument(final WebUrl webUrl) {
    try {
      Document document = Jsoup.connect(webUrl.getUrl()).userAgent(this
          .userAgent)
          .timeout(20 * 1000).get();
      return document;
    } catch (IOException e) {
      logger.warn("Get document failed.", webUrl);
    }
    return null;
  }

  protected List<WebUrl> getUrls(final WebUrl webUrl, final Document document) {
    if (webUrl.getDepth() >= this.seed.getDepth()) {
      return null;
    }
    List<WebUrl> list = new ArrayList<>();
    Elements links = document.getElementsByTag("a");
    if (links.isEmpty()) {
      return null;
    }
    links.forEach(e -> {
      String url = e.absUrl("href");
      if (!this.filter.contains(url)) {
        WebUrl childUrl = new WebUrl(webUrl.getDepth() + 1, url);
        childUrl.setDepth(webUrl.getDepth() + 1);
        childUrl.setUrl(url);
        if (parser.shouldVisit(webUrl, childUrl) && isMatched(url)) {
          list.add(childUrl);
        }
        this.filter.add(url);
      }
    });
    return list;
  }

  protected boolean isMatched(String url) {
    for (String pattern : this.seed.getPatterns()) {
      if (WildcardMatcher.match(url, pattern)) {
        return true;
      }
    }
    return false;
  }
}
