/**
 * date   : 2016年1月29日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import com.iveely.brain.environment.Variable;
import com.iveely.brain.mind.Brain;
import com.iveely.framework.file.Directory;

import org.apache.log4j.Logger;
import org.dom4j.Document;
import org.dom4j.Element;
import org.dom4j.io.SAXReader;

import java.io.File;
import java.util.List;

/**
 * @author {Iveely Liu}
 */
public class CategoryLoader {

  /**
   * Logger.
   */
  private static Logger logger;

  /**
   * Single instance.
   */
  private static CategoryLoader loader;

  private CategoryLoader() {
    logger = Logger.getLogger(CategoryLoader.class);
  }

  /**
   * Get single instance.
   */
  public static CategoryLoader getInstance() {
    if (loader == null) {
      synchronized (CategoryLoader.class) {
        if (loader == null) {
          loader = new CategoryLoader();
        }
      }
    }
    return loader;
  }

  /**
   * Load all categories.
   */
  public void load() {
    // 1. Get path.
    String path = Variable.getPathOfCategory();

    // 2. Get all files.
    File[] files = Directory.getFiles(path);
    if (files != null) {
      // 2.1 Use XML to parse.
      SAXReader reader = new SAXReader();
      for (File file : files) {
        try {
          Document document = reader.read(file);
          Element root = document.getRootElement();
          List<Element> childElements = root.elements();
          for (Element element : childElements) {
            Category category = new Category();
            if (category.build(element)) {
              Brain.getInstance().addCategory(category);
            } else {
              logger.error(String.format("category parse error,%s", element.asXML()));
            }
          }
          logger.info(String.format("Process category %s finished.", file.getName()));
        } catch (Exception e) {
          logger.error(String.format("XML parse error,file name is '%s'", file.getName()));
          e.printStackTrace();
        }
      }
    }
  }
}
