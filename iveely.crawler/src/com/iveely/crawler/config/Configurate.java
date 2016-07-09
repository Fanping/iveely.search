package com.iveely.crawler.config;

import com.iveely.framework.text.JSONUtil;

import java.io.File;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class Configurate {

  private String indexUrl;


  public String getIndexUrl() {
    return indexUrl;
  }

  public void setIndexUrl(String indexUrl) {
    this.indexUrl = indexUrl;
  }

  private static Configurate instance;

  private static Object mutex = new Object();

  public static Configurate get() {
    if (instance == null) {
      synchronized (mutex) {
        if (instance == null) {
          instance = JSONUtil.fromFile(new File("conf/system.setting"),
              Configurate.class);
        }
      }
    }
    return instance;
  }
}
