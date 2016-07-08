package com.iveely.crawler.common;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class WildcardMatcher {

  /**
   * Tag to indicate if the strings should be compared case sensitive or not.
   */
  private static boolean ignoreCase = false;

  /**
   * Empty construct, since all the methods are static, you don't ha ve to make
   * an object of this class, and thus I hide the construtor in private.
   */
  private WildcardMatcher() {
  }

  /**
   * Give the boolean value whether the given file name and the wild card
   * matches.
   *
   * @param string   The source string to be compared to the wildcard.
   * @param wildcard The wildcard.
   * @return True, if they matches.
   */
  public static boolean match(String string, String wildcard) {
    return match(string, wildcard, false);
  }

  /**
   * Give the boolean value whether the given file name and the wild card
   * matches.
   *
   * @param string     The source string to be compared to the wildcard.
   * @param wildcard   The wildcard.
   * @param ignorecase If this param is true, all the comparison is made not
   *                   casesensitive.
   * @return True, if they matches.
   */
  public static boolean match(String string, String wildcard, boolean
      ignorecase) {
    ignoreCase = ignorecase;
    if (wildcard.indexOf("*") < 0)
      return stringEquals(string, wildcard);
    if (wildcard.equals("*"))
      return true;
    String sub = null;
    int pw = 0, ps = 0, wlen = wildcard.length(), slen = string.length(), index;
    boolean first = true;
    while (pw < wlen) {
      index = wildcard.indexOf('*', pw);
      if (index < 0) {
        sub = wildcard.substring(pw);
        pw = wlen; // to skip out
      } else {
        sub = wildcard.substring(pw, index);
        pw = index + 1;
      }
      index = find(string, sub, ps);
      if (first) {
        first = false;
        if (index != 0) {
          return false;
        } else ps = sub.length();
      } else {
        if (index < 0) {
          return false;
        }
        ps = index + sub.length();
      }
    }
    if (wildcard.charAt(wildcard.length() - 1) != '*') {
      index = find(string, sub, string.length() - sub.length());
      if (index < 0)
        return false;
    }
    return true;
  }

  /**
   * Inner method to test whether two string equals, it is same as S
   * tring.equals, except that it supports wildcard "?"
   *
   * @param a A string to compare
   * @param b A string to compare
   * @return True If a and b equals, notice that this could be invol ve wildcard
   * "?"
   */
  private static boolean stringEquals(String a, String b) {
    int alen = a.length(), blen = b.length();
    if (alen != blen)
      return false;
    if (find(a, b, 0) == 0)
      return true;
    return false;
  }

  /**
   * Find the match position of the two string, it is same as String .indexOf,
   * except that it supports wildcard "?"
   *
   * @param string   The source string to be compared to the pattern.
   * @param pattern  The wildcard contained string.
   * @param startpos The start position of this search.
   * @return The match position of the two string,-1 if they do not match.
   */
  private static int find(String string, String pattern, int startpos) {
    if (pattern.equals("")) {
      return startpos;
    }
    int orgp, pp = 0, ps = startpos, strlen = string.length(), patternlen =
        pattern.length();
    char pc, sc;
    boolean cmatch;
    orgp = ps;
    while (ps < strlen) {
      sc = string.charAt(ps);
      pc = pattern.charAt(pp);
      if (ignoreCase) {
        cmatch = (charEqualsIgnoreCase(sc, pc) || pc == '?');
      } else {
        cmatch = (sc == pc || pc == '?');
      }
      if (!cmatch) {
        pp = 0;
        ps = ++orgp;
        continue;
      }
      pp++;
      ps++;
      if (pp == patternlen) {
        return orgp;
      }
    }
    return -1;
  }

  /**
   * Compare two char but ignore their case.
   *
   * @param a Char to compare.
   * @param b Char to compare.
   * @return True, if they equals or they equals while ignoring their case.
   */
  private static boolean charEqualsIgnoreCase(char a, char b) {
    if (a == b)
      return true;
    char t;
    if (a > b) {
      t = a;
      a = b;
      b = t;
    } // now a<b
    int at = getCharType(a);
    int bt = getCharType(b);
    if (at == UPPERCASE && bt == LOWERCASE && a + 'a' - 'A' == b)
      return true;
    return false;
  }

  /**
   * This is a inner method called by charEqualsIgnoreCase, which gi ves the
   * type of the cha.
   *
   * @param c The char to find type.
   * @return LOWERCASE--if the char is in lowercase. UPPERCASE--if the char is
   * in uppercase. NOT_A_LETTER--if the char is not a letter.
   */
  private static int getCharType(char c) {
    if (c >= 'a' && c <= 'z') {
      return LOWERCASE;
    }
    if (c >= 'A' && c <= 'Z') {
      return UPPERCASE;
    }
    return NOT_A_LETTER;
  }

  /**
   * Constant value for char type that is in lowercase.
   */
  final private static int LOWERCASE = 10;

  /**
   * Constant value for char type that is in uppercase.
   */
  final private static int UPPERCASE = 20;

  /**
   * Constant value for char type that is not a letter.
   */
  final private static int NOT_A_LETTER = 30;

}
