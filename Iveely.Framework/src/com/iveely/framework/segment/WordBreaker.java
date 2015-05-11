package com.iveely.framework.segment;

import com.iveely.nlp.segment.Segmenter;

/**
 * Word breaker for Iveely.Framework.
 *
 * @author liufanping@iveely.com
 * @date 2015-1-24 22:09:00
 */
public class WordBreaker {

    /**
     * Single instance.
     */
    private static WordBreaker breaker;

    private static Segmenter segmenter;

    private WordBreaker() {
        segmenter = Segmenter.getInstance();
    }

    public static WordBreaker getInstance() {
        if (breaker == null) {
            breaker = new WordBreaker();
        }
        return breaker;
    }

    /**
     * Split string to array.
     *
     * @param sentence
     * @return
     */
    public String[] splitToArray(String sentence) {
        return segmenter.splitToArray(sentence);
    }

    /**
     * Split with delimeter.
     *
     * @param sentence
     * @param delimeter
     * @return
     */
    public String split(String sentence, String delimeter) {
        String[] result = splitToArray(sentence);
        return String.join(delimeter, result);
    }
}
