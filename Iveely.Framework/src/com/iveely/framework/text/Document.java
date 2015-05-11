package com.iveely.framework.text;

import java.io.UnsupportedEncodingException;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

/**
 * Documents Related.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-9 11:19:14
 */
public class Document {

    /**
     * Computing document similarity.
     *
     * @param doc1
     * @param doc2
     * @return
     */
    public static double getSimilarity(String doc1, String doc2) {
        if (doc1 != null && doc1.trim().length() > 0 && doc2 != null
                && doc2.trim().length() > 0) {
            Map<Integer, int[]> AlgorithmMap = new HashMap<>();
            for (int i = 0; i < doc1.length(); i++) {
                char d1 = doc1.charAt(i);
                if (isChinese(d1)) {
                    int charIndex = getGB2312Id(d1);
                    if (charIndex != -1) {
                        int[] fq = AlgorithmMap.get(charIndex);
                        if (fq != null && fq.length == 2) {
                            fq[0]++;
                        } else {
                            fq = new int[2];
                            fq[0] = doc1.length() - i;
                            fq[1] = 0;
                            AlgorithmMap.put(charIndex, fq);
                        }
                    }
                }
            }

            for (int i = 0; i < doc2.length(); i++) {
                char d2 = doc2.charAt(i);
                if (isChinese(d2)) {
                    int charIndex = getGB2312Id(d2);
                    if (charIndex != -1) {
                        int[] fq = AlgorithmMap.get(charIndex);
                        if (fq != null && fq.length == 2) {
                            fq[1]++;
                        } else {
                            fq = new int[2];
                            fq[0] = 0;
                            fq[1] = doc2.length() - i;
                            AlgorithmMap.put(charIndex, fq);
                        }
                    }
                }
            }

            Iterator<Integer> iterator = AlgorithmMap.keySet().iterator();
            double sqdoc1 = 0;
            double sqdoc2 = 0;
            double denominator = 0;
            while (iterator.hasNext()) {
                int[] c = AlgorithmMap.get(iterator.next());
                denominator += c[0] * c[1];
                sqdoc1 += c[0] * c[0];
                sqdoc2 += c[1] * c[1];
            }

            return denominator / Math.sqrt(sqdoc1 * sqdoc2);
        } else {
            return 0;
        }
    }

    /**
     * Is chinese word.
     *
     * @param ch
     * @return
     */
    public static boolean isChinese(char ch) {
        return (ch >= 0x4E00 && ch <= 0x9FA5);

    }

    /**
     * According Unicode character input, access to its code or ascii encoded
     * GB2312.
     *
     * @param ch
     * @return
     */
    public static short getGB2312Id(char ch) {
        try {
            byte[] buffer = Character.toString(ch).getBytes("GB2312");
            if (buffer.length != 2) {
                return -1;
            }
            int b0 = (int) (buffer[0] & 0x0FF) - 161;
            int b1 = (int) (buffer[1] & 0x0FF) - 161;
            return (short) (b0 * 94 + b1);
        } catch (UnsupportedEncodingException e) {
        }
        return -1;
    }
}
