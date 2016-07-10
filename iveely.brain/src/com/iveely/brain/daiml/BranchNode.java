/**
 * date   : 2016年1月31日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import com.iveely.brain.mind.Brain;

import org.dom4j.Element;

import java.util.ArrayList;
import java.util.List;

/**
 * @author {Iveely Liu}
 */
public class BranchNode {

  /**
   * Name of the server to send request.
   */
  private String name;

  /**
   * The parameter send to the server.
   */
  private String parameter;

  /**
   * Index id of star.
   */
  private List<Integer> list;

  public BranchNode() {
    this.list = new ArrayList<>();
  }

  /**
   * @return the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * @return the parameter
   */
  public String getParameter(List<String> stars) {
    if (this.list.size() > 0) {
      String ret = this.parameter;
      for (Integer id : list) {
        ret = ret.replace("%s" + id + "%", stars.get(id - 1));
      }
      return ret;
    } else {
      return this.parameter;
    }
  }

  /**
   * Parse to request information.
   */
  public boolean parse(Element element) {
    if (element == null) {
      return false;
    }

    // 1. Get name of branch,and check it.
    String nav = element.attributeValue("node");
    if (!Brain.getInstance().isBranchExist(nav)) {
      return false;
    }
    setName(nav);

    // 2. Get parameter.
    List<Element> children = element.elements();
    if (children.size() > 0) {
      for (Element child : children) {
        String tag = child.getName();
        if (tag.equals("star")) {
          int id = Integer.parseInt(child.attributeValue("index"));
          list.add(id);
          child.setText("%s" + id + "%");
        } else {
          return false;
        }
      }
    }
    this.parameter = element.getStringValue().trim();
    return true;
  }
}
