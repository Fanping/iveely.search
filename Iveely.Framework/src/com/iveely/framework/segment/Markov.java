package com.iveely.framework.segment;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import org.apache.log4j.Logger;

/**
 * Hidden Markov Model
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 10:27:43
 */
public class Markov {

    /**
     * Semantic count.
     */
    private static final int SEMANTIC_COUNT = 101;

    /**
     * id->semantic table.
     */
    private final String[] id2Semantic = new String[SEMANTIC_COUNT + 1];

    /**
     * semantic->id table.
     */
    private final TreeMap<String, Integer> semantic2Id;

    /**
     * user's dictionary.
     */
    private String userDir;

    /**
     * Semantic weight.
     */
    private final int[] semanticWeight;

    /**
     * Transition probability.
     */
    private final int[][] transitionProbability;

    /**
     * Conditional shift.
     */
    private final double[][] p = new double[SEMANTIC_COUNT + 1][SEMANTIC_COUNT + 1];

    /**
     * Root of trie.
     */
    private final Tnode root;

    /**
     * Single instance.
     */
    private static Markov segment;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Markov.class.getName());

    private Markov() {
        this.semantic2Id = new java.util.TreeMap<>();
        this.root = new Tnode();
        this.transitionProbability = new int[SEMANTIC_COUNT][SEMANTIC_COUNT];
        this.semanticWeight = new int[SEMANTIC_COUNT];
        learn();
    }

    /**
     * Trie.
     */
    public static class Tnode {

        public static final int k = 102;

        /**
         * Children nodes.
         */
        public java.util.TreeMap<Character, Tnode> childNodes;

        /**
         * Is enable.
         */
        public boolean isEnable;

        /**
         * Number of corpus call this.
         */
        public int count;

        /**
         * Speech may be the node corresponding.
         */
        public int[] semanticCount = new int[k];

        public Tnode() {
            isEnable = false;
            childNodes = new java.util.TreeMap<>();
            count = 0;
        }
    }

    /**
     * DP status indicates.
     */
    public static class Transition {

        public String str = "";
        public static final int k = 102;
        public double[] each = new double[k];
        public int[] last = new int[k];
    }

    public static Markov getInstance() {
        if (segment == null) {
            segment = new Markov();
        }
        return segment;
    }

    /**
     * Insert node.
     *
     * @param observValue
     * @param semanticId
     */
    private void insertNode(String observValue, int semanticId) {
        Tnode currentRoot = root;
        int ptr = 0;
        while (ptr < observValue.length()) {
            char t = observValue.charAt(ptr);
            if (!currentRoot.childNodes.containsKey(t)) {
                currentRoot.childNodes.put(t, new Tnode());
            }
            currentRoot = currentRoot.childNodes.get(t);
            ptr++;
        }
        currentRoot.isEnable = true;
        currentRoot.count++;
        currentRoot.semanticCount[semanticId]++;
    }

    /**
     * Init semantic.
     */
    private void initSemantic() {
        semantic2Id.put("nz", 0);
        id2Semantic[0] = "nz";
        semantic2Id.put("t", 1);
        id2Semantic[1] = "t";
        semantic2Id.put("ng", 2);
        id2Semantic[2] = "ng";
        semantic2Id.put("p", 3);
        id2Semantic[3] = "p";
        semantic2Id.put("w", 4);
        id2Semantic[4] = "w";
        semantic2Id.put("nsf", 5);
        id2Semantic[5] = "nsf";
        semantic2Id.put("v", 6);
        id2Semantic[6] = "v";
        semantic2Id.put("n", 7);
        id2Semantic[7] = "n";
        semantic2Id.put("ude1", 8);
        id2Semantic[8] = "ude1";
        semantic2Id.put("f", 9);
        id2Semantic[9] = "f";
        semantic2Id.put("mq", 10);
        id2Semantic[10] = "mq";
        semantic2Id.put("vn", 11);
        id2Semantic[11] = "vn";
        semantic2Id.put("cc", 12);
        id2Semantic[12] = "cc";
        semantic2Id.put("uyy", 13);
        id2Semantic[13] = "uyy";
        semantic2Id.put("d", 14);
        id2Semantic[14] = "d";
        semantic2Id.put("a", 15);
        id2Semantic[15] = "a";
        semantic2Id.put("c", 16);
        id2Semantic[16] = "c";
        semantic2Id.put("rz", 17);
        id2Semantic[17] = "rz";
        semantic2Id.put("qt", 18);
        id2Semantic[18] = "qt";
        semantic2Id.put("m", 19);
        id2Semantic[19] = "m";
        semantic2Id.put("vi", 20);
        id2Semantic[20] = "vi";
        semantic2Id.put("gm", 21);
        id2Semantic[21] = "gm";
        semantic2Id.put("ntc", 22);
        id2Semantic[22] = "ntc";
        semantic2Id.put("nrf", 23);
        id2Semantic[23] = "nrf";
        semantic2Id.put("uls", 24);
        id2Semantic[24] = "uls";
        semantic2Id.put("nis", 25);
        id2Semantic[25] = "nis";
        semantic2Id.put("x", 26);
        id2Semantic[26] = "x";
        semantic2Id.put("vshi", 27);
        id2Semantic[27] = "vshi";
        semantic2Id.put("rzv", 28);
        id2Semantic[28] = "rzv";
        semantic2Id.put("uzhe", 29);
        id2Semantic[29] = "uzhe";
        semantic2Id.put("nnd", 30);
        id2Semantic[30] = "nnd";
        semantic2Id.put("ude3", 31);
        id2Semantic[31] = "ude3";
        semantic2Id.put("ns", 32);
        id2Semantic[32] = "ns";
        semantic2Id.put("nnt", 33);
        id2Semantic[33] = "nnt";
        semantic2Id.put("nr", 34);
        id2Semantic[34] = "nr";
        semantic2Id.put("pbei", 35);
        id2Semantic[35] = "pbei";
        semantic2Id.put("ule", 36);
        id2Semantic[36] = "ule";
        semantic2Id.put("vf", 37);
        id2Semantic[37] = "vf";
        semantic2Id.put("ad", 38);
        id2Semantic[38] = "ad";
        semantic2Id.put("s", 39);
        id2Semantic[39] = "s";
        semantic2Id.put("pba", 40);
        id2Semantic[40] = "pba";
        semantic2Id.put("an", 41);
        id2Semantic[41] = "an";
        semantic2Id.put("qv", 42);
        id2Semantic[42] = "qv";
        semantic2Id.put("udeng", 43);
        id2Semantic[43] = "udeng";
        semantic2Id.put("b", 44);
        id2Semantic[44] = "b";
        semantic2Id.put("k", 45);
        id2Semantic[45] = "k";
        semantic2Id.put("vx", 46);
        id2Semantic[46] = "vx";
        semantic2Id.put("rr", 47);
        id2Semantic[47] = "rr";
        semantic2Id.put("vl", 48);
        id2Semantic[48] = "vl";
        semantic2Id.put("vyou", 49);
        id2Semantic[49] = "vyou";
        semantic2Id.put("ry", 50);
        id2Semantic[50] = "ry";
        semantic2Id.put("vd", 51);
        id2Semantic[51] = "vd";
        semantic2Id.put("z", 52);
        id2Semantic[52] = "z";
        semantic2Id.put("bl", 53);
        id2Semantic[53] = "bl";
        semantic2Id.put("nt", 54);
        id2Semantic[54] = "nt";
        semantic2Id.put("uzhi", 55);
        id2Semantic[55] = "uzhi";
        semantic2Id.put("vg", 56);
        id2Semantic[56] = "vg";
        semantic2Id.put("r", 57);
        id2Semantic[57] = "r";
        semantic2Id.put("q", 58);
        id2Semantic[58] = "q";
        semantic2Id.put("nf", 59);
        id2Semantic[59] = "nf";
        semantic2Id.put("al", 60);
        id2Semantic[60] = "al";
        semantic2Id.put("rzs", 61);
        id2Semantic[61] = "rzs";
        semantic2Id.put("uguo", 62);
        id2Semantic[62] = "uguo";
        semantic2Id.put("dl", 63);
        id2Semantic[63] = "dl";
        semantic2Id.put("nto", 64);
        id2Semantic[64] = "nto";
        semantic2Id.put("gi", 65);
        id2Semantic[65] = "gi";
        semantic2Id.put("usuo", 66);
        id2Semantic[66] = "usuo";
        semantic2Id.put("ude2", 67);
        id2Semantic[67] = "ude2";
        semantic2Id.put("gg", 68);
        id2Semantic[68] = "gg";
        semantic2Id.put("nrj", 69);
        id2Semantic[69] = "nrj";
        semantic2Id.put("ag", 70);
        id2Semantic[70] = "ag";
        semantic2Id.put("tg", 71);
        id2Semantic[71] = "tg";
        semantic2Id.put("ulian", 72);
        id2Semantic[72] = "ulian";
        semantic2Id.put("dg", 73);
        id2Semantic[73] = "dg";
        semantic2Id.put("y", 74);
        id2Semantic[74] = "y";
        semantic2Id.put("ryv", 75);
        id2Semantic[75] = "ryv";
        semantic2Id.put("nhd", 76);
        id2Semantic[76] = "nhd";
        semantic2Id.put("gb", 77);
        id2Semantic[77] = "gb";
        semantic2Id.put("j", 78);
        id2Semantic[78] = "j";
        semantic2Id.put("rys", 79);
        id2Semantic[79] = "rys";
        semantic2Id.put("ntu", 80);
        id2Semantic[80] = "ntu";
        semantic2Id.put("l", 81);
        id2Semantic[81] = "l";
        semantic2Id.put("nit", 82);
        id2Semantic[82] = "nit";
        semantic2Id.put("o", 83);
        id2Semantic[83] = "o";
        semantic2Id.put("nmc", 84);
        id2Semantic[84] = "nmc";
        semantic2Id.put("u", 85);
        id2Semantic[85] = "u";
        semantic2Id.put("gp", 86);
        id2Semantic[86] = "gp";
        semantic2Id.put("ryt", 87);
        id2Semantic[87] = "ryt";
        semantic2Id.put("nhm", 88);
        id2Semantic[88] = "nhm";
        semantic2Id.put("udh", 89);
        id2Semantic[89] = "udh";
        semantic2Id.put("rzt", 90);
        id2Semantic[90] = "rzt";
        semantic2Id.put("Rg", 91);
        id2Semantic[91] = "Rg";
        semantic2Id.put("gc", 92);
        id2Semantic[92] = "gc";
        semantic2Id.put("e", 93);
        id2Semantic[93] = "e";
        semantic2Id.put("Mg", 94);
        id2Semantic[94] = "Mg";
        semantic2Id.put("ntcb", 95);
        id2Semantic[95] = "ntcb";
        semantic2Id.put("i", 96);
        id2Semantic[96] = "i";
        semantic2Id.put("nba", 97);
        id2Semantic[97] = "nba";
        semantic2Id.put("na", 98);
        id2Semantic[98] = "na";
        semantic2Id.put("nbc", 99);
        id2Semantic[99] = "nbc";
        semantic2Id.put("nr1", 100);
        id2Semantic[100] = "nr1";
        semantic2Id.put("nr2", 101);
        id2Semantic[101] = "nr2";

    }

    /**
     * Learn from corpus.
     */
    private void learn() {
        buildTrieTree("Resources/Split_Corpus.txt");
    }

    /**
     * Load user's dictionary.
     *
     * @param words
     */
    public void setUserDic(String words) {
        this.userDir = words;
    }

    /**
     * Build trie.
     *
     * @param filePath
     */
    private void buildTrieTree(String filePath) {

        // 1. 初始化词性对照表
        initSemantic();
        try {
            try (BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(filePath), "UTF-8"))) {
                String line;
                while ((line = reader.readLine()) != null) {
                    line = line.replace("  ", " ");
                    line = line.replace("  ", " ");
                    line = line.replace("  ", " ");
                    String[] wordAndSemantic = line.replace(" ", "@").split("[@]", -1);
                    int last = -1;
                    for (String singleWords : wordAndSemantic) {
                        if (singleWords.length() == 0) {
                            break;
                        }
                        String[] word = singleWords.split("[/]", -1);
                        if (word.length == 2) {
                            try {
                                int semanticIndex = semantic2Id.get(word[1]);
                                if (semanticIndex < SEMANTIC_COUNT) {
                                    insertNode(word[0], semanticIndex);
                                    semanticWeight[semanticIndex]++;
                                    if (last != -1) {
                                        transitionProbability[last][semanticIndex]++;
                                    }
                                    last = semanticIndex;
                                }
                            } catch (java.lang.Exception e) {
                                logger.error(e);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < SEMANTIC_COUNT; ++i) {
                for (int j = 0; j < SEMANTIC_COUNT; ++j) {
                    p[i][j] = transitionProbability[i][j] * 1.0 / semanticWeight[j];
                }
            }
            for (int i = 0; i <= SEMANTIC_COUNT; ++i) {
                p[SEMANTIC_COUNT][i] = p[i][SEMANTIC_COUNT] = 1.0;
            }
        } catch (IOException e) {
            logger.error(e);
        }
    }

    /**
     * Split.
     *
     * @param text
     * @return
     */
    public final Tuple splitToArray(String text) {
        try {
            String[] sentences = splitByUserDic(text);
            java.util.ArrayList<String> words = new java.util.ArrayList<>();
            java.util.ArrayList<String> semantics = new java.util.ArrayList<>();
            for (String content : sentences) {
                if (content.length() == 0) {
                    continue;
                }
                if (content.contains("/user")) {
                    words.add(content.replace("/user", ""));
                    semantics.add("user");
                    continue;
                }
                int ptr = 0;
                Transition[] ans = new Transition[10005];
                int s = 0;
                while (ptr < content.length()) {
                    if (content.charAt(ptr) == '\r' && content.charAt(ptr + 1) == '\n') {
                        ptr += 2;
                        continue;
                    }
                    ans[s] = new Transition();
                    int deep = 0;
                    Tnode trie = root;
                    for (int i = 0; i + ptr < content.length(); ++i) {
                        char t = content.charAt(i + ptr);
                        if (!trie.childNodes.containsKey(t)) {
                            break;
                        }
                        trie = trie.childNodes.get(t);
                        if (trie.isEnable) {
                            deep = i + 1;
                            ans[s].str = "";
                            for (int j = 0; j < deep; ++j) {
                                ans[s].str += content.charAt(ptr + j);
                            }
                            if (s == 0) {
                                for (int j = 0; j <= SEMANTIC_COUNT; ++j) {
                                    if (trie.count > 0) {
                                        ans[s].each[j] = trie.semanticCount[j] * 1.0 / trie.count;
                                    } else {
                                        ans[s].each[j] = 0;
                                    }
                                    ans[s].last[j] = -1;
                                }
                            } else {
                                for (int j = 0; j < SEMANTIC_COUNT; ++j) {
                                    int save = -1;
                                    double big = -1;
                                    for (int r = 0; r <= SEMANTIC_COUNT; ++r) {
                                        if (ans[s - 1].each[r] * p[r][j] > big) {
                                            big = ans[s - 1].each[r] * p[r][j];
                                            save = r;
                                        }
                                    }
                                    ans[s].each[j] = big * trie.semanticCount[j] * 1.0 / trie.count;
                                    ans[s].last[j] = save;
                                }
                                ans[s].each[SEMANTIC_COUNT] = 0;
                            }
                        }
                    }
                    if (deep == 0) {
                        deep++;
                        for (int j = 0; j < SEMANTIC_COUNT; ++j) {
                            ans[s].each[j] = 0;
                        }
                        ans[s].each[SEMANTIC_COUNT] = 1;
                        ans[s].str = "";
                        for (int j = 0; j < deep; ++j) {
                            ans[s].str += content.charAt(ptr + j);
                        }
                        if (s == 0) {
                            ans[s].last[SEMANTIC_COUNT] = -1;
                        } else {
                            int save = -1;
                            double big = -1;
                            for (int r = 0; r <= SEMANTIC_COUNT; ++r) {
                                if (ans[s - 1].each[r] > big) {
                                    big = ans[s - 1].each[r];
                                    save = r;
                                }
                            }
                            ans[s].last[SEMANTIC_COUNT] = save;
                        }
                    }
                    ptr += deep;
                    s++;
                }
                int[] optlist = new int[10005];
                double bb = -1;
                for (int i = 0; i <= SEMANTIC_COUNT; ++i) {
                    if (ans[s - 1].each[i] > bb) {
                        optlist[s - 1] = i;
                        bb = ans[s - 1].each[i];
                    }
                }
                for (int i = s - 1; i >= 1; --i) {
                    int da, db;
                    da = optlist[i];
                    db = ans[i].last[da];
                    optlist[i - 1] = db;
                }

                String lastSemantic = "";
                String temp = "";
                for (int i = 0; i < s; ++i) {
                    if ("x".equals(id2Semantic[optlist[i]])) {
                        temp += ans[i].str;
                    } else {
                        if (!"".equals(temp)) {
                            words.add(temp);
                            semantics.add("x");
                            temp = "";
                        }
                        words.add(ans[i].str);
                        semantics.add(id2Semantic[optlist[i]]);
                    }
                    lastSemantic = id2Semantic[optlist[i]];
                }
                if (!"".equals(temp)) {
                    words.add(temp);
                    semantics.add("x");
                }
            }
            return new Tuple(words.toArray(new String[]{}), semantics.toArray(new String[]{}));
        } catch (Exception e) {
            logger.error(text + ":" + e);
        }
        return null;
    }

    /**
     * Split with semantic.
     *
     * @param text
     * @param delimeter
     * @return
     */
    public final String splitWithSemantic(String text, String delimeter) {
        Tuple tuple = splitToArray(text);
        StringBuilder buffer = new StringBuilder();
        for (int i = 0; i < tuple.getTLength(); i++) {
            buffer.append(tuple.getTStr(i)).append("/").append(tuple.getVStr(i)).append(delimeter);
        }
        return buffer.toString();
    }

    public final String split(String text, String delimeter) {
        Tuple tuple = splitToArray(text);
        StringBuilder buffer = new StringBuilder();
        for (int i = 0; i < tuple.getTLength(); i++) {
            buffer.append(tuple.getTStr(i)).append(delimeter);
        }
        return buffer.toString();
    }

    public final String[] split(String text) {
        Tuple tuple = splitToArray(text);
        if (tuple != null) {
            return tuple.getT();
        }
        return null;
    }

    /**
     * Split by user's dictionary.
     *
     * @param text
     * @return
     */
    public String[] splitByUserDic(String text) {
        if (userDir != null && userDir.length() > 0) {
            Pattern pattern = Pattern.compile(userDir);
            Matcher m = pattern.matcher(text);
            String[] words = pattern.split(text);
            List<String> data = new ArrayList<>();
            if (words.length > 0) {
                int count = 0;
                while (count < words.length) {
                    if (m.find()) {
                        data.add(words[count]);
                        data.add(m.group() + "/user");
                    } else {
                        data.add(words[count]);
                    }
                    count++;
                }
            }
            words = new String[data.size()];
            words = data.toArray(words);
            return words;
        }
        String[] words = new String[1];
        words[0] = text;
        return words;
    }
}
