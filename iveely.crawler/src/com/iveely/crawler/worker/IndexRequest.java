package com.iveely.crawler.worker;

import com.iveely.crawler.config.Configurate;
import com.iveely.crawler.entity.Article;
import com.iveely.framework.index.StoreField;
import com.iveely.framework.index.StoredDocument;
import com.iveely.framework.text.JSONUtil;

import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.DefaultHttpClient;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class IndexRequest {

  private static Logger logger = LoggerFactory.getLogger(IndexRequest.class);

  public static void post(final Article article) {
    if (article == null
        || article.getTitle() == null
        || article.getContent() == null
        || article.getUrl() == null) {
      return;
    }
    try {
      List<StoreField> storeFields = new ArrayList<>();
      storeFields.add(new StoreField<>(
          StoreField.Type.TEXT,
          "title",
          article.getTitle()));
      storeFields.add(new StoreField<>(
          StoreField.Type.STRING,
          "url",
          article.getUrl()));
      storeFields.add(new StoreField<>(
          StoreField.Type.TEXT,
          "content",
          article.getContent()
      ));
      storeFields.add(new StoreField<>(
          StoreField.Type.LONG,
          "timestamp",
          article.getCrawlTimestamp()
      ));
      String text = JSONUtil.toString(new StoredDocument(storeFields));
      HttpClient httpclient = new DefaultHttpClient();
      HttpPost httpPost = new HttpPost(Configurate.get().getIndexUrl());
      httpPost.setHeader("Content-Type", "application/json;charset=UTF-8");
      httpPost.setEntity(new StringEntity(text, "UTF-8"));
      httpclient.execute(httpPost);
      httpclient.getConnectionManager().shutdown();
    } catch (Exception ex) {
      logger.warn("Post exception.", ex);
    }
  }
}
