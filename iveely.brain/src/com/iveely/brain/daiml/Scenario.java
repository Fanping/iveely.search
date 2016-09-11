/**
 * date   : 2016年2月2日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import org.dom4j.Element;

import java.util.ArrayList;
import java.util.List;

/**
 * @author {Iveely Liu}
 */
public class Scenario {

  /**
   * Expression of scenario.
   */
  private String express;

  /**
   * Index id of star.
   */
  private List<Integer> sids;

  /**
   * index id of node.
   */
  private List<Integer> nids;

  public Scenario() {
    this.sids = new ArrayList<>();
    this.nids = new ArrayList<>();
  }

  /**
   * Parse element to script.
   *
   * @param element element
   * @return whether parse success
   */
  public boolean parse(Element element) {
    List<Element> children = element.elements();
    for (Element child : children) {
      String tag = child.getName();
      if (tag.equals("star")) {
        int id = Integer.parseInt(child.attributeValue("index"));
        this.sids.add(id);
        child.setText("%s" + id + "%");
      } else if (tag.equals("node")) {
        int id = Integer.parseInt(child.attributeValue("index"));
        this.nids.add(id);
        child.setText("%n" + id + "%");
      } else if (tag.equals("ret")) {
        child.setText("%r%");
      } else {
        return false;
      }
    }
    this.express = element.getStringValue().trim();
    return true;
  }

  /**
   * Get script content.
   *
   * @param stars stars
   * @param ret   ret
   * @param nodes nodes
   * @return script
   */
  public String getScript(List<String> stars, String ret, List<String> nodes) {
    String result = express.replace("%r%", ret);
    for (Integer id : sids) {
      result = result.replace("%s" + id + "%", stars.get(id - 1));
    }
    for (Integer id : nids) {
      result = result.replace("%n" + id + "%", nodes.get(id - 1));
    }
    return result;
  }

}
