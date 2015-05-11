/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.

 ROOT：要处理文本的语句                                                                      IP：简单从句

 NP：名词短语                                                                                      VP：动词短语

 

 

 PU：断句符，通常是句号、问号、感叹号等标点符号                                   LCP：方位词短语

 PP：介词短语                                                                                      CP：由‘的’构成的表示修饰性关系的短语

 DNP：由‘的’构成的表示所属关系的短语                                               ADVP：副词短语

 ADJP：形容词短语                                                                               DP：限定词短语

 QP：量词短语                                                                                     NN：常用名词

 NR：固有名词                                                                                     NT：时间名词

 PN：代词                                                                                           VV：动词

 VC：是                                                                                              CC：不是（应该是吧！！不太确定）

 VE：有                                                                                              VA：表语形容词

 AS：内容标记（如：了）                                                                     VRD：动补复合词
 */
package com.iveely.framework.text.syntactic;

import com.iveely.framework.text.Triple;
import edu.stanford.nlp.ling.CoreLabel;
import edu.stanford.nlp.ling.Sentence;
import edu.stanford.nlp.ling.TaggedWord;
import edu.stanford.nlp.parser.lexparser.LexicalizedParser;
import edu.stanford.nlp.trees.GrammaticalStructure;
import edu.stanford.nlp.trees.GrammaticalStructureFactory;
import edu.stanford.nlp.trees.Tree;
import edu.stanford.nlp.trees.TreebankLanguagePack;
import edu.stanford.nlp.trees.TypedDependency;
import com.iveely.nlp.segment.HMMSematic;
import com.iveely.nlp.segment.Tuple;
import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

/**
 *
 * @author X1 Carbon
 */
public class Parser {

    /**
     * Lexical parser.
     */
    private static LexicalizedParser parser;

    /**
     * Lexical tree.
     */
    private Tree root;

    /**
     * Meet the requirements of nodes.
     */
    private List<Tree> validNodes;

    public Parser() {
        parser = LexicalizedParser.loadModel("resources/lexparser/chineseFactored.ser.gz");
        validNodes = new ArrayList<>();
    }

    /**
     * Query node which has two nodes. one is :ADVP one is :VP and parent is VP.
     */
    public List<Triple> input(String line, String sentence, String[] content) {

        // Sentence parse.
        List<CoreLabel> rawWords = Sentence.toCoreLabelList(content);
        if (root == null) {
            root = parser.apply(rawWords);
        }

        // Semantic parse.
//        Tuple tuple = HMMSematic.getInstance().splitToArray(sentence);
//        String[] semantics = tuple.getV();
        int shouldGo = 0;
        String firstEntity = "";
        String lastTag = "NN";
        boolean shouldLastNN = true;
        List<TaggedWord> tags = root.taggedYield();
        if (isGoodStartTag(tags.get(0).tag()) && isGoodStartValue(rawWords.get(0).value())) {
            for (int i = 0; i < rawWords.size(); i++) {
                String tag = tags.get(i).tag();
                if ("NR".equals(tag) || "NT".equals(tag)) {
                    if (firstEntity.isEmpty()) {
                        firstEntity = tag;
                    }
                    shouldGo++;
                }
            }
        }

        if ((shouldGo > 1 && "NR".equals(firstEntity)) || (shouldGo >= 1 && "NN".equals(tags.get(0).tag()))) {
            // 1. Trave root to find effect nodes.
            travelTree(root);

            // 2. Process tree.
            return processTree(line);
        }
        return null;
    }

    /**
     * NP sentence and VP sentence should all have.
     *
     * @param content
     * @return
     */
    public boolean isNPtoVP(String[] content) {
        // Sentence parse.
        if (root == null) {
            List<CoreLabel> rawWords = Sentence.toCoreLabelList(content);
            root = parser.apply(rawWords);
        }
        if (root.children().length == 1 && "IP".equals(root.children()[0].value())) {
            if (checkNPtoVP(root.children()[0])) {
                //SyntacticAnalysis.getInstance().parse(root);
                return true;
            }
        }
        return false;
    }

    public boolean isNPtoNP(String[] content) {
        if (root == null) {
            List<CoreLabel> rawWords = Sentence.toCoreLabelList(content);
            root = parser.apply(rawWords);
        }
        if (root.children().length == 1 && "NP".equals(root.children()[0].value())) {
            if (checkNPtoNP(root.children()[0])) {
               // SyntacticAnalysis.getInstance().parse(root);
                return true;
            }
        }
        return false;
    }

    public void clear() {
        validNodes.clear();
        root = null;
    }

    /**
     * Check NP and VP.
     *
     * @param node
     * @return
     */
    private boolean checkNPtoVP(Tree node) {
        Tree[] nodes = node.children();
        boolean isNP = false;
        boolean isVP = false;
        for (int i = 0; i < nodes.length; i++) {
            if (nodes[i].value().equals("NP")) {
                isNP = !isNP && checkNP(nodes[i]);
            } else if (nodes[i].value().equals("VP")) {
                isVP = !isVP && checkVP(nodes[i]);
            } else {
                return false;
            }
        }
        return isNP && isVP;
    }

    private boolean checkNPtoNP(Tree node) {
        Tree[] nodes = node.children();
        if (nodes.length >= 2) {
            return nodes[0].value().equals("NP") && nodes[nodes.length - 1].value().equals("NP");
        } else {
            return false;
        }
    }

    /**
     * 1. NP can not contains 'PN'
     *
     * @param node
     * @return
     */
    private boolean checkNP(Tree node) {
        List<Tree> lefts = node.getLeaves();
        int ntCount = 0;
        for (int i = 0; i < lefts.size(); i++) {
            String val = lefts.get(i).parent(root).value();
            if (val.equals("PN")) {
                return false;
            }
            if (val.equals("DT")) {
                return false;
            }
            if (val.equals("NT")) {
                ntCount++;
            }
        }
        // under NP can not only NT no matter how many lefts.
        if (ntCount == lefts.size()) {
            return false;
        }

        // under NP can not only NT or NN when 1 size.
        if (lefts.size() == 1) {
            String tag = lefts.get(0).parent(root).value();
            if (tag.equals("NT") || tag.equals("NN")) {
                return false;
            }
        }

        // NP can not start with "DT"
        String startTag = lefts.get(0).parent(root).value();
        if (startTag.equals("AD")
                || startTag.equals("JJ")
                || startTag.equals("VE")) {
            return false;
        }

        // NP can not start with "(" && "（"
        String startWords = lefts.get(0).value();
        if (startWords.equals("(")
                || startWords.equals("（")
                || startWords.equals("•")) {
            return false;
        }

        return true;
    }

    /**
     * 1. VP can not end with 'VV'. 2. VP can not contains 'PN'
     *
     * @param node
     * @return
     */
    private boolean checkVP(Tree node) {
        List<Tree> lefts = node.getLeaves();
        if (lefts.size() < 2) {
            return false;
        }
        String endTag = lefts.get(lefts.size() - 1).parent(root).value();
        if (endTag.equals("LC")) {
            return false;
        }
        for (int i = 0; i < lefts.size(); i++) {
            String tag = lefts.get(i).parent(root).value();
            if (tag.equals("PN")
                    || tag.equals("DT")) {
                return false;
            }
        }
        return true;
    }

    /**
     * Travel tree.
     *
     * @param root
     * @param node
     */
    private void travelTree(Tree node) {
        if (isValidNode(node)) {

            // Binary tree.
            if (node.children().length == 2) {
                String childValue = node.children()[0].value();
                if (node.children()[1].children() != null && node.children()[1].children().length == 2) {
                    if (isIncludeEntity(node.children()[0]) && isIncludeEntity(node.children()[1])) {
                        validNodes.add(node);
                    }
                }
                if (node.children()[1].children() != null && node.children()[1].children().length > 2) {
                    if (node.children()[1].lastChild().value().equals("VP") && isIncludeEntity(node.children()[1].lastChild())) {
                        validNodes.add(node);
                    }
                }
            }

            // triple tree.
            if (node.children().length == 3) {
                String childValue = node.children()[0].value();
                if (node.children()[1].children() != null && node.children()[2].children() != null) {
                    if (isIncludeEntity(node.children()[0]) && isIncludeEntity(node.children()[2])) {
                        validNodes.add(node);
                    }
                }
            }
        }
        Tree[] nodes = node.children();
        if (nodes != null) {
            boolean isValid = true;
            for (int i = 0; i < nodes.length; i++) {
                if ("DEC".equals(nodes[i].value())) {
                    isValid = false;
                }
            }
            for (int i = 0; i < nodes.length && isValid; i++) {
                travelTree(nodes[i]);
            }
        }
    }

    /**
     * Process nodes.
     */
    private List<Triple> processTree(String line) {
        List<Triple> list = new ArrayList<>();
        for (int i = 0; i < validNodes.size() && i < 1; i++) {
            if (validNodes.get(i).children().length == 2) {

                if (validNodes.get(i).children()[1].children().length == 2) {
                    String entityA = getAllNodeValue(validNodes.get(i).children()[0], "").trim();
                    String relation = getAllNodeValue(validNodes.get(i).children()[1].children()[0], "").trim();
                    String entityB = getAllNodeValue(validNodes.get(i).children()[1].children()[1], "").trim();

                    Triple triple = new Triple();
                    triple.setEntityA(entityA);
                    triple.setRelation(relation);
                    triple.setEntityB(entityB);
                    if (isValid(triple)) {
                        addDetails(triple, line);
                        list.add(triple);
                    }
                }

                if (validNodes.get(i).children()[1].children().length > 2) {
                    String entityA = getAllNodeValue(validNodes.get(i).children()[0], "").trim();
                    String relation = "";
                    for (int m = 0; m < validNodes.get(i).children()[1].children().length - 1; m++) {
                        relation += getAllNodeValue(validNodes.get(i).children()[1].children()[m], "").trim();
                    }

                    String entityB = getAllNodeValue(validNodes.get(i).children()[1].lastChild(), "").trim();
                    Triple triple = new Triple();
                    triple.setEntityA(entityA);
                    triple.setRelation(relation);
                    triple.setEntityB(entityB);
                    if (isValid(triple)) {
                        addDetails(triple, line);
                        list.add(triple);
                    }
                }

            }
            if (validNodes.get(i).children().length == 3) {
                String entityA = getAllNodeValue(validNodes.get(i).children()[0], "").trim();
                String relation = getAllNodeValue(validNodes.get(i).children()[1], "").trim();
                String entityB = getAllNodeValue(validNodes.get(i).children()[2], "").trim();
                Triple triple = new Triple();
                triple.setEntityA(entityA);
                triple.setRelation(relation);
                triple.setEntityB(entityB);
                if (isValid(triple)) {
                    addDetails(triple, line);
                    list.add(triple);
                }
            }
        }
        return list;
    }

    /**
     * Get leaf value.
     *
     * @param node
     * @param val
     * @return
     */
    private String getAllNodeValue(Tree node, String val) {
        String nv = node.value();
        if (node.isLeaf()) {
            return node.value();
        }
        if (node != null && node.children() != null) {
            for (int i = 0; i < node.children().length; i++) {
                val += getAllNodeValue(node.children()[i], "");
            }
        }
        return val;
    }

    /**
     * Is include entity for a node.
     *
     * @param node
     * @return
     */
    private boolean isIncludeEntity(Tree node) {
        if ("NP".equals(node.value())) {
            return true;
        }
        Tree[] nodes = node.children();
        if (nodes != null) {
            for (int i = 0; i < nodes.length; i++) {
                if (isIncludeEntity(nodes[i])) {
                    return true;
                }
            }
        }
        return false;
    }

    public String getEntity(String[] data) {
        List<CoreLabel> rawWords = Sentence.toCoreLabelList(data);
        Tree node = parser.apply(rawWords);

        List<TaggedWord> tags = node.taggedYield();
        String lastTag = "NN";
        String entityName = "";
        boolean isAwalysNN = true;
        for (int i = 0; i < tags.size() && (i < 1 || isAwalysNN); i++) {
            String tag = tags.get(i).tag();
            if ("NR".equals(tag)) {
                return tags.get(i).value();
            }
            if ("NN".equals(tag) && "NN".equals(lastTag) && isAwalysNN) {
                entityName += tags.get(i).value();
            } else {
                isAwalysNN = false;
            }
        }
        return entityName;
    }

    private boolean isValid(Triple triple) {
        return triple.getEntityA().length() >= 1 && triple.getEntityB().length() >= 1 && triple.getRelation().length() >= 1 && !"：".equals(triple.getRelation()) && !":".equals(triple.getRelation()) && !triple.getEntityB().contains("：") && !triple.getEntityB().contains(":");
    }

    private boolean isValidNode(Tree node) {
        if (("VP".equals(node.value()) || "IP".equals(node.value())) && node.children() != null) {
            return true;
        }
        return false;
    }

    private boolean isGoodStartTag(String tag) {
        if (tag.equals("AD")) {
            return false;
        }
        return true;
    }

    private boolean isGoodStartValue(String val) {
        if (val.equals("（") || val.equals("(")) {
            return false;
        }
        return true;
    }

    private void addDetails(Triple triple, String line) {
//        Tuple tuple = HMMSematic.getInstance().splitToArray(triple.toSimple());
//        for (int i = 0; i < tuple.getVLength(); i++) {
//            if ("nr".equals(tuple.getVStr(i))) {
//                triple.addEntity(tuple.getTStr(i));
//            }
//            if ("t".equals(tuple.getVStr(i))) {
//                triple.addTime(tuple.getTStr(i));
//            }
//            if ("ns".equals(tuple.getVStr(i))) {
//                triple.addLocation(tuple.getTStr(i));
//            }
//        }
//        tuple = HMMSematic.getInstance().splitToArray(line);
//        boolean hasNr = triple.isEntitesEmpty();
//        boolean hasT = triple.isTimesEmpty();
//        boolean hasLocations = triple.isLocationsEmpty();
//        for (int i = 0; i < tuple.getVLength(); i++) {
//            if (hasNr) {
//                if ("nr".equals(tuple.getVStr(i))) {
//                    triple.addEntity(tuple.getTStr(i));
//                }
//            }
//
//            if (hasT) {
//                if ("t".equals(tuple.getVStr(i))) {
//                    triple.addTime(tuple.getTStr(i));
//                }
//            }
//
//            if (hasLocations) {
//                if ("ns".equals(tuple.getVStr(i))) {
//                    triple.addLocation(tuple.getTStr(i));
//                }
//            }
//        }
    }
}
