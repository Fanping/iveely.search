/**
 * date   : 2016年1月27日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.daiml;

import com.iveely.brain.daiml.Star.StarType;
import com.iveely.brain.mind.Brain;
import com.iveely.framework.text.StringUtil;

import org.apache.log4j.Logger;

import java.util.ArrayList;
import java.util.List;
import java.util.regex.Matcher;

/**
 * @author {Iveely Liu}
 */
public class Pattern {

  /**
   * Logger.
   */
  private static Logger logger;
  /**
   * The value of the pattern.
   */
  private String[] array;
  /**
   * The flags of the unknown(set or star or together).
   */
  private Star[] stars;
  /**
   * The pattern weather ready.
   */
  private boolean ready;
  /**
   * Regular expression to check.
   */
  private java.util.regex.Pattern checker;
  /**
   * That information.
   */
  private String that;

  public Pattern(String val, String that) {
    Pattern.logger = Logger.getLogger(Pattern.class);
    this.ready = this.parse(val);
    this.that = that;
  }

  /**
   * Check the pattern is ready.
   *
   * @return true is ready, or is not.
   */
  public boolean isReady() {
    return this.ready;
  }

  /**
   * The question is match the pattern.
   *
   * @param question The question from user.
   * @return true is matched, false is not.
   */
  public boolean isMatch(String question, List<String> stars, String that) {
    if (this.that != null && !this.that.equals(that)) {
      return false;
    }

    // 1. Check ready.
    if (!this.ready) {
      logger.error(String.format("The pattern has some error:%s.", getValue()));
      return false;
    }

    // 2. Check null.
    if (question == null || question.length() < 1) {
      logger.error(String.format("Question can not be null."));
      return false;
    }

    // 3. Split.
    String[] qs = StringUtil.split(question);

    // 4. Check match.
    return checkMathed(qs) && extractStar(question, stars) && checkType(stars);
  }

  /**
   * Get value of the pattern.
   *
   * @return The String value of pattern.
   */
  public String getValue() {
    StringBuffer buffer = new StringBuffer();
    for (String ar : array) {
      buffer.append(ar);
    }
    return buffer.toString();
  }

  /**
   * Get words for index.
   */
  public String getIndex() {
    return this.checker.toString().replace("(.*)", "");
  }

  /**
   * Parse pattern to understandable expression. [*]\[set]
   */
  private boolean parse(String val) {
    if (val == null) {
      logger.error(String.format("Pattern value can not be null."));
      return false;
    } else {
      String patVal = "";
      this.array = StringUtil.split(val.trim());
      List<Star> list = new ArrayList<>();
      short last = -1;
      for (int i = 0; i < this.array.length; i++) {
        if (this.array[i].equals("*")) {
          if (last == 1) {
            logger.error(String.format("'*' cannot be continuous use in [%s].", val));
            return false;
          } else if (last == 2) {
            logger.error(String.format("'*' and 'set' cannot be continuous use in [%s].", val));
            return false;
          } else {
            last = 1;
            list.add(getStarType(i));
            patVal += "(.*)";
            if (i != this.array.length - 1) {
              if (this.array[i + 1].startsWith("[") && this.array[i + 1].endsWith("]")) {
                i++;
              }
            }
          }
        } else if (this.array[i].equals("<set>")) {
          if (last == 2) {
            logger.error(String.format("'set' cannot be continuous use in [%s].", val));
            return false;
          } else if (last == 1) {
            logger.error(String.format("'*' and 'set' cannot be continuous use in [%s].", val));
            return false;
          } else {
            i++;
            String name = "";
            while (i < this.array.length) {
              if (this.array[i].equals("</set>")) {
                break;
              } else {
                name += this.array[i++];
              }
            }
            last = 2;
            list.add(new Star(StarType.SET, name));
            patVal += "(.*)";
          }
        } else {
          last = 0;
          patVal += this.array[i];
        }
      }
      try {
        this.checker = java.util.regex.Pattern.compile(patVal);
      } catch (Exception e) {
        logger.error(e);
        return false;
      }

      stars = new Star[list.size()];
      stars = list.toArray(stars);
      return true && this.checker != null;
    }
  }

  /**
   * Simple check the text weather is matched.
   *
   * @param text The text to be check.
   * @return True is matched,or is not.
   */
  private boolean checkMathed(String[] text) {
    boolean ret = false;
    String cur;
    boolean isLastStar = false;
    int backi = 0;
    int backj = 0;
    int i, j;
    for (i = 0, j = 0; i < text.length; ) {
      if (this.array.length <= j) {
        if (backi != 0) {
          isLastStar = true;
          i = backi;
          j = backj;
          backi = 0;
          backj = 0;
          continue;
        }
        break;
      }

      if ((cur = this.array[j]).equals("*")) {
        if (j == this.array.length - 1) {
          ret = true;
          break;
        }
        if (this.array[j + 1].startsWith("[") && this.array[j + 1].endsWith("]")) {
          j++;
        }
        j++;
        isLastStar = true;
        continue;
      }

      if (cur.equals("<set>")) {
        while (j < this.array.length) {
          if (this.array[j].equals("</set>")) {
            j++;
            break;
          }
          j++;
        }
        if (j == this.array.length - 1) {
          ret = true;
          break;
        }
        isLastStar = true;
        continue;
      }

      if (isLastStar) {
        if (text[i].equals(cur)) {
          isLastStar = false;
          backi = i + 1;
          backj = j;
          j++;
        }
      } else {
        if (!cur.equals("?") && !cur.equals(text[i])) {
          ret = false;
          if (backi != 0) {
            isLastStar = true;
            i = backi;
            j = backj;
            backi = 0;
            backj = 0;
            continue;
          }
          break;
        }
        j++;
      }
      i++;
    }

    if (i == text.length && j == this.array.length)
      ret = true;
    return ret;
  }

  /**
   * Extract value of the star.
   *
   * @param text The text to be extract.
   * @param list The values of the star.
   * @return True is successfully extracted.
   */
  private boolean extractStar(String text, List<String> list) {
    try {
      Matcher m = this.checker.matcher(text);
      while (m.find()) {
        for (int i = 1; i <= stars.length; i++) {
          list.add(m.group(i));
        }
      }
    } catch (Exception e) {
      logger.error(e);
      return false;
    }
    return true;
  }

  /**
   * To check weather the set matched for current pattern.
   *
   * @param text The array of string to be check.
   * @return true is matched,or is not.
   */
  private boolean checkType(List<String> vals) {
    if (vals.size() == stars.length) {
      for (int i = 0; i < stars.length; i++) {
        boolean isValid = true;
        if (stars[i].getType().equals(Star.StarType.EVERYTHING)) {
          continue;
        } else if (stars[i].getType().equals(Star.StarType.NUMBER)) {
          isValid = StringUtil.isNumber(vals.get(i));
        } else if (stars[i].getType().equals(Star.StarType.SET)) {
          isValid = Brain.getInstance().isInSet(stars[i].getValue(), vals.get(i));
        } else if (stars[i].getType().equals(Star.StarType.DATETIME)) {
          isValid = StringUtil.isDateTime(vals.get(i));
        } else if (stars[i].getType().equals(Star.StarType.EN_WORD)) {
          isValid = StringUtil.isEnWord(vals.get(i));
        } else if (stars[i].getType().equals(Star.StarType.EMAIL)) {
          isValid = StringUtil.isEmail(vals.get(i));
        } else if (stars[i].getType().equals(Star.StarType.TELEPHONE)) {
          isValid = StringUtil.isTelNumber(vals.get(i));
        } else if (stars[i].getType().equals(Star.StarType.IPADDRESS)) {
          isValid = StringUtil.isIPAddress(vals.get(i));
        } else if (stars[i].getType().equals(Star.StarType.URL)) {
          isValid = StringUtil.isUrl(vals.get(i));
        } else {
          isValid = false;
        }
        if (!isValid) {
          return false;
        }
      }
      return true;
    }
    return false;
  }

  /**
   * Get star type by index.
   *
   * @param i The index of the star.
   * @return Type of the star.
   */
  private Star getStarType(int i) {
    if (i == this.array.length - 1) {
      return new Star(StarType.EVERYTHING);
    }
    String val = this.array[i + 1].toUpperCase();
    if (val.equals("[NUMBER]")) {
      return new Star(StarType.NUMBER);
    } else if (val.equals("[DATETIME]")) {
      return new Star(StarType.DATETIME);
    } else if (val.equals("[EN_WORD]")) {
      return new Star(StarType.EN_WORD);
    } else if (val.equals("[EMAIL]")) {
      return new Star(StarType.EMAIL);
    } else if (val.equals("[TELEPHONE]")) {
      return new Star(StarType.TELEPHONE);
    } else if (val.equals("[IPADDRESS]")) {
      return new Star(StarType.IPADDRESS);
    } else if (val.equals("[URL]")) {
      return new Star(StarType.URL);
    } else {
      return new Star(StarType.EVERYTHING);
    }
  }
}
