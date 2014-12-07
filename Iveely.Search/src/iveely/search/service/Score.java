package iveely.search.service;

/**
 * Score
 *
 * @author liufanping@iveely.com
 * @date 2014-11-17 20:39:01
 */
public class Score {

    /**
     * Get idf normal value when not found.
     *
     * @return
     */
    public static double getIdfNormal() {
        return 0.75;
    }

    /**
     * When same page was select from diff keywords, the weight should change.
     *
     * @return
     */
    public static double getRelative() {
        return 100;
    }

    public static double getChildWordRelative() {
        return 1.5;
    }

    public static double getChildWordRank() {
        return 0.01;
    }

    public static double getTitleScore() {
        return 8;
    }

    public static double getUrlScore() {
        return 0.1;
    }

    public static double getContentSocre() {
        return 18;
    }
}
