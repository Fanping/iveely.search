/**
 * date   : 2016年1月27日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import com.iveely.brain.mind.React;
import com.iveely.brain.mind.React.Status;

import org.dom4j.Element;

import java.util.ArrayList;
import java.util.List;

/**
 * @author {Iveely Liu}
 */
public class Category {

  /**
   * Pattern of the category.
   */
  private Pattern pattern;

  /**
   * Template of the category.
   */
  private ITemplate template;

  public Category() {

  }

  /**
   * Get words to index.
   *
   * @return the index text.
   */
  public String getIndex() {
    if (pattern != null) {
      return pattern.getIndex();
    }
    return null;
  }

  /**
   * Build pattern and template.
   *
   * @param element The XML element.
   * @return true is successfully build,or is not.
   */
  public boolean build(Element element) {
    boolean ret = true;
    // 1. Get children.
    List<Element> list = element.elements();

    // 2. Parse pattern and template.
    for (Element ele : list) {
      String tag = ele.getName().toLowerCase();
      if (tag.equals("pattern")) {
        // 2.1 pattern.
        pattern = new Pattern(ele.getStringValue(), ele.attributeValue("that"));
      } else if (tag.equals("template")) {
        // 2.2 template.
        List<Element> children = ele.elements();
        if (isNormal(children)) {
          template = new TNormal();
          ret = ret && template.parse(ele);
        } else if (isSrai(children)) {
          template = new TSrai();
          ret = ret && template.parse(children.get(0));
        } else if (isRandom(children)) {
          template = new TRandom();
          ret = ret && template.parse(children.get(0));
        } else if (isRequest(children)) {
          template = new TRequest();
          ret = ret && template.parse(ele);
        }
        ret = ret && (template != null && pattern != null);
      }
    }
    return ret;
  }

  /**
   * Get the answer of the question in category.
   *
   * @param question the question to get answer.
   * @param that last question
   * @return the reaction of the answer.
   */
  public React getAnswer(String question, String that) {
    // 1. Use pattern to check.
    List<String> stars = new ArrayList<>();
    if (pattern.isMatch(question, stars, that)) {
      React react = new React(template.getStatus());
      react.setRet(template.getResult(stars, that));
      react.setThat(pattern.getValue());
      return react;
    } else {
      return new React(Status.FAILURE);
    }
  }

  /**
   * Check is normal template.
   * @param list list
   */
  private boolean isNormal(List<Element> list) {
    if (list.size() == 0) {
      return true;
    }
    for (Element element : list) {
      String tag = element.getName();
      if (!tag.equals("star")) {
        return false;
      }
    }
    return true;
  }

  /**
   * Check is srai tempalte.
   * @param list list
   */
  private boolean isSrai(List<Element> list) {
    if (list.size() == 1) {
      String tag = list.get(0).getName();
      if (tag.equals("srai")) {
        return true;
      }
    }
    return false;
  }

  /**
   * Check is random template.
   * @param list list
   */
  private boolean isRandom(List<Element> list) {
    if (list.size() == 1) {
      String tag = list.get(0).getName();
      if (tag.equals("random")) {
        return true;
      }
    }
    return false;
  }

  /**
   * Check is request template.
   * @param list list
   */
  private boolean isRequest(List<Element> list) {
    if (list.size() > 1) {
      String tag = list.get(0).getName();
      if (tag.equals("request")) {
        return true;
      }
    }
    return false;
  }
}
