/**
 * date   : 2016年1月29日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import com.iveely.brain.mind.React.Status;

import org.dom4j.Element;

import java.util.ArrayList;
import java.util.List;

/**
 * @author {Iveely Liu}
 */
public class TNormal extends ITemplate {

  /**
   * Text of the answer.
   */
  private String text;

  /**
   * Type of the template.
   */
  private Status status;

  /**
   * Star for replace.
   */
  private List<Integer> ids;

  public TNormal() {
    status = Status.SUCCESS;
    ids = new ArrayList<>();
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.robot.daiml.ITemplate#getType()
   */
  public Status getStatus() {
    return status;
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.robot.daiml.ITemplate#getValue()
   */
  public String getResult(List<String> stars, String that) {
    return replaceStar(this.text, this.ids, stars);
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.robot.daiml.ITemplate#parse(org.dom4j.Element)
   */
  @Override
  public boolean parse(Element element) {
    List<Element> children = element.elements();
    for (Element child : children) {
      String tag = child.getName();
      if (tag.equals("star")) {
        int id = Integer.parseInt(child.attributeValue("index"));
        ids.add(id);
        child.setText("%s" + id + "%");
      }
    }
    this.text = element.getStringValue().trim();
    return true;
  }
}
