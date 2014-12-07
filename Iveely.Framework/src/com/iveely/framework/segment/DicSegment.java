package com.iveely.framework.segment;

import com.iveely.framework.file.Reader;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import org.apache.log4j.Logger;

/**
 * Segment by dictionary.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-25 21:49:14
 */
public class DicSegment {

    /**
     * Dictionary.
     */
    private final HashSet<String> dictionary;

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(DicSegment.class.getName());

    /**
     * Single instanlce.
     */
    private static DicSegment segment;

    private DicSegment() {
        dictionary = new HashSet<>();
        List<String> lines = Reader.readAllLine("Common/Dic.txt", "UTF-8");
        lines.stream().forEach((line) -> {
            dictionary.add(line);
        });
    }

    public static DicSegment bug_getInstance() {
        if (segment == null) {
            logger.info("load dic.");
            segment = new DicSegment();
        }
        return segment;
    }

    public static void clear() {
        if (segment != null) {
            segment.dictionary.clear();
            segment = null;
        }
    }

    public String[] bug_split(String input, boolean findChildWord) {
        try {
            List<String> result = new ArrayList<>();
            String[] inputStrings = input.split(" |\\t|\\n|�");
            for (String str : inputStrings) {
                String sentence = str.trim();
                int senLength = sentence.length();
                if (senLength > 0) {
                    if (senLength > 100) {
                        int currentIndex = 0;
                        int step = 100;
                        while (currentIndex < senLength - 1) {
                            split(sentence.substring(currentIndex, step), result, findChildWord);
                            currentIndex += 100;
                            step += 100;
                            if (currentIndex + 100 > senLength) {
                                step = sentence.length() - 1;
                            }
                        }
                    } else {
                        split(str.trim(), result, findChildWord);
                    }
                }
            }
            boolean islastEngChar = false;
            String temp = "";
            List<String> sortedResult = new ArrayList<>();
            for (int i = result.size() - 1; i > -1; i--) {
                if (result.get(i).equals(" ")) {
                    if (!temp.equals("")) {
                        sortedResult.add(temp.toLowerCase());
                        temp = "";
                    }
                    continue;
                }
                char ca = result.get(i).toCharArray()[0];
                if ((ca <= 'Z' && ca >= 'A')
                        || (ca <= 'z' && ca >= 'a')) {
                    temp += result.get(i);
                    islastEngChar = true;
                } else {
                    if (islastEngChar) {
                        sortedResult.add(temp.toLowerCase());
                        temp = "";
                    }
                    islastEngChar = false;
                    sortedResult.add(result.get(i));
                }
            }
            if (!"".equals(temp)) {
                sortedResult.add(temp);
            }
            String[] words = new String[sortedResult.size()];
            words = sortedResult.toArray(words);
            return words;
        } catch (Exception e) {
            logger.error(e);
            return null;
        }
    }

    /**
     * Split words.
     *
     * @param input
     * @return
     */
    private List<String> split(String input, List<String> result, boolean findChildWord) {
        if (input == null) {
            return result;
        }
        String temp = null;
        for (int i = 0; i < input.length(); i++) {
            temp = input.substring(i);
            if (dictionary.contains(temp)) {
                if (!findChildWord || (!(input == null ? temp == null : input.equals(temp)))) {
                } else {
                    continue;
                }
                result.add(temp);
                input = input.substring(0, i);
                i = -1;
            } else if (temp.length() == 1) {
                result.add(temp);
            }
        }

        if (null != input && !"".equals(input)) {
            input = input.substring(0, input.length() - 1);
            this.split(input, result, findChildWord);
        }
        return result;
    }
}
