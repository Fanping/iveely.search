package com.iveely.framework.segment;

import java.util.ArrayList;
import java.util.List;

/**
 * NGram term sequence
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 10:48:48
 */
public class NGram {

    /**
     * Using One-Gram term.
     *
     * @param text
     * @return
     */
    public static String[] splitByUnigram(String text) {
        List<String> list = new ArrayList<>();
        char[] data = text.toCharArray();
        boolean islastEngChar = false;
        String temp = "";
        for (int i = 0; i < data.length; i++) {
            if (Character.isWhitespace(data[i])) {
                if (!temp.equals("")) {
                    list.add(temp.toLowerCase());
                    temp = "";
                }
                continue;
            }
            if ((data[i] <= 'Z' && data[i] >= 'A')
                    || (data[i] <= 'z' && data[i] >= 'a')) {
                temp += data[i];
                islastEngChar = true;
            } else {
                if (islastEngChar) {
                    list.add(temp.toLowerCase());
                    temp = "";
                }
                islastEngChar = false;
                list.add(String.valueOf(data[i]));
            }
        }
        if (!temp.equals("")) {
            list.add(temp.toLowerCase());
            temp = "";
        }
        String[] result = new String[list.size()];
        result = list.toArray(result);
        return result;
    }
}
