package com.iveely.crawler.entity;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class WebUrl {

  private Integer depth;

  private String url;

  public WebUrl(final Integer depth,final String url){
    setDepth(depth);
    setUrl(url);
  }

  public Integer getDepth() {
    return depth;
  }

  public void setDepth(Integer depth) {
    this.depth = depth;
  }

  public String getUrl() {
    return url;
  }

  public void setUrl(String url) {
    this.url = url;
  }
}
