/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.framework.text;

import com.iveely.framework.text.syntactic.Parser;
import com.iveely.framework.segment.WordBreaker;
import java.util.ArrayList;
import java.util.List;

/**
 * Knowledge extractor.
 *
 * @author X1 Carbon
 */
public class KnowledgeExtractor {

    /**
     * Single instance parser.
     */
    private static com.iveely.framework.text.syntactic.Parser parser;

    /**
     * Single instance extractor.
     */
    private static KnowledgeExtractor extractor;

    private KnowledgeExtractor() {
        parser = new Parser();
    }

    /**
     * Get single instance.
     *
     * @return
     */
    public static KnowledgeExtractor getInstance() {
        if (extractor == null) {
            extractor = new KnowledgeExtractor();
        }
        return extractor;
    }

    /**
     * Parse text to knowledge.
     *
     * @param text
     * @return
     */
    public List<Triple> parse(String text) {
        String[] lines = text.split("\\.|。|\\?|！|？|!|\\t|\\r|\\n|\\^");
        List<Triple> all = new ArrayList<>();
        for (String line : lines) {
            String data = line.trim();
            List<String> sentences = replaceSubject(line);
            for (String sentence : sentences) {
                if (isVaildSentence(sentence)) {
                    String[] content = WordBreaker.getInstance().splitToArray(sentence.trim());
                    System.out.println(String.join(" ", content));
//                    if (content.length >= 3 && content.length <= 30) {
//                        List<Triple> temp = parser.input(line, sentence, content);
//                        if (temp != null && temp.size() > 0) {
//                            all.addAll(temp);
//                        }
//                    }
                    parser.clear();
                    if (parser.isNPtoVP(content) || parser.isNPtoNP(content)) {
                        System.out.println(String.join(" ", content));

//                        List<Triple> temp = parser.input(line, sentence, content);
//                        if (temp != null && temp.size() > 0) {
//                            all.addAll(temp);
//                        }
//                        parser.clear();
                    }
                }
            }
        }
        return all;
    }

    /**
     * Replace 'it','she','he'
     *
     * @param line
     * @return
     */
    private List<String> replaceSubject(String line) {
        List<String> list = new ArrayList<>();
        String[] sentences = line.replace(" ", "").trim().split(",|，|；");
        String mainEntity = "";
        if (sentences.length == 1) {
            if (isNotIgnor(sentences[0], true)) {
                list.add(line);
            }
        } else {
            String[] firstResult = WordBreaker.getInstance().splitToArray(sentences[0]);
            if (firstResult.length > 0) {
                mainEntity = parser.getEntity(firstResult);
            }
            list.add(sentences[0]);
            for (int i = 1; i < sentences.length; i++) {
                String myEntity = parser.getEntity(WordBreaker.getInstance().splitToArray(sentences[i]));
                if (myEntity.isEmpty()) {
                    if (mainEntity.isEmpty()) {
                    } else {
                        sentences[i] = mainEntity + sentences[i];
                        if (isNotIgnor(sentences[i], false)) {
                            sentences[i] = sentences[i].replace("他", mainEntity).replace("她", myEntity).replace("它", myEntity);
                        } else {
                            continue;
                        }
                        list.add(sentences[i]);
                    }
                } else {
                    if (isNotIgnor(sentences[i], true)) {
                        list.add(sentences[i]);

                    } else if (isNotIgnor(sentences[i], false) && !myEntity.isEmpty()) {
                        sentences[i] = sentences[i].replace("他", mainEntity).replace("她", myEntity).replace("它", myEntity);
                        list.add(sentences[i]);
                    } else {
                        continue;
                    }

                }
            }
        }
        return list;
    }

    private boolean isNotIgnor(String str, boolean forceZero) {
        int flag = -1;
        if (str.contains("他")) {
            flag++;
        }
        if (str.contains("她")) {
            flag++;
        }
        if (str.contains("它")) {
            flag++;
        }
        return (flag < 1 && !forceZero) || (forceZero && flag == -1);
    }

    private boolean isVaildSentence(String sentence) {
        if (sentence.trim().length() < 5) {
            return false;
        }
        if (sentence.contains("[")
                || sentence.contains("【")
                || sentence.contains("、")
                || sentence.endsWith("嘛")) {
            return false;
        }
        return true;
    }

}
