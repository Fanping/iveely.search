/**
 * date   : 2016年1月29日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import com.iveely.brain.mind.React.Status;

import org.dom4j.Element;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

/**
 * @author {Iveely Liu}
 */
public class TRandom extends ITemplate {

  /**
   * All possible answers.
   */
  private List<String> list;

  /**
   * All star id of each li.
   */
  private List<List<Integer>> ids;

  /**
   * Type of the template.
   */
  private Status status;

  public TRandom() {
    this.status = Status.SUCCESS;
    this.list = new ArrayList<>();
    this.ids = new ArrayList<>();
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.robot.daiml.ITemplate#parse(java.lang.String)
   */
  public boolean parse(Element element) {
    List<Element> children = element.elements();
    if (children.size() == 0) {
      return false;
    } else {
      for (Element child : children) {
        String tag = child.getName();
        if (tag.equals("li")) {
          List<Element> cs = child.elements();
          if (cs.size() > 0) {
            List<Integer> ls = new ArrayList<>();
            for (Element ele : cs) {
              String ctname = ele.getName();
              if (ctname.equals("star")) {
                int id = Integer.parseInt(ele.attributeValue("index"));
                ls.add(id);
                ele.setText("%s" + id + "%");
                list.add(child.getStringValue().trim());
              }
            }
            ids.add(ls);
          } else {
            list.add(child.getStringValue().trim());
            ids.add(null);
          }

        } else {
          return false;
        }
      }
    }
    return true;
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.robot.daiml.ITemplate#getType()
   */
  public Status getStatus() {
    return this.status;
  }

  /*
   * (non-Javadoc)
   *
   * @see com.iveely.robot.daiml.ITemplate#getResult()
   */
  public String getResult(List<String> stars, String that) {
    int size = list.size();
    int id = new Random().nextInt(size) % (size + 1);
    return replaceStar(list.get(id), ids.get(id), stars);
  }

}
