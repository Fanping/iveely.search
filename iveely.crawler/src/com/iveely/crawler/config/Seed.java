package com.iveely.crawler.config;

import org.apache.commons.lang3.StringUtils;

import java.io.Serializable;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class Seed implements Serializable {

  private String name;

  private String url;

  private String[] patterns;

  private Integer depth;

  private String userAgent;

  private Integer frequency;

  public String getUrl() {
    return url;
  }

  public void setUrl(String url) {
    this.url = url;
  }

  public String[] getPatterns() {
    return patterns;
  }

  public void setPatterns(String[] patterns) {
    this.patterns = patterns;
  }

  public Integer getDepth() {
    return depth;
  }

  public void setDepth(Integer depth) {
    this.depth = depth;
  }

  public String getUserAgent() {
    return userAgent;
  }

  public void setUserAgent(String userAgent) {
    this.userAgent = userAgent;
  }

  public Integer getFrequency() {
    return frequency;
  }

  public void setFrequency(Integer frequency) {
    this.frequency = frequency;
  }

  public String getName() {
    return name;
  }

  public void setName(String name) {
    this.name = name;
  }

  public Seed() {

  }
}
