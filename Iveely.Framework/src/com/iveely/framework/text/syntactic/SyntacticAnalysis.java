package com.iveely.framework.text.syntactic;

import com.iveely.framework.segment.WordBreaker;
import com.iveely.framework.text.Triple;
import edu.stanford.nlp.ling.CoreLabel;
import edu.stanford.nlp.ling.Sentence;
import edu.stanford.nlp.parser.common.ArgUtils;
import edu.stanford.nlp.parser.lexparser.LexicalizedParser;
import edu.stanford.nlp.trees.Tree;
import java.util.ArrayList;
import java.util.List;

/**
 * Syntactic analysis for a sentence.
 *
 * @author liufanping@iveely.com
 * @date 2015-2-12 21:12:27
 */
public class SyntacticAnalysis {

    public enum Type {

        Format_NR_NNT,
        Format_NR_NT,
        Format_VC_VV_P_DEG,
        ALL
    }

    /**
     * Lexical parser.
     */
    private static LexicalizedParser parser;

    /**
     * All formats.
     */
    private final List<IFormat> formats;

    private SyntacticAnalysis() {

        formats = new ArrayList<>();

        // 1. NR-NNT
        formats.add(new Format_NR_NNT());

        // 2. NR-NT
        formats.add(new Format_NR_NT());

        // 3. *-VC-*-VV-P-*-DEG-*
        formats.add(new Format_VC_VV_P_DEG());
    }

    /**
     * Single instance.
     */
    private static SyntacticAnalysis analysis;

    public static void setDataFolder(String dataFolder) {
        ArgUtils.dataFolder = dataFolder;
    }

    /**
     * Get instance.
     *
     * @return
     */
    public static SyntacticAnalysis getInstance() {
        if (analysis == null) {
            parser = LexicalizedParser.loadModel(ArgUtils.dataFolder + "resources/lexparser/chineseFactored.ser.gz");
            analysis = new SyntacticAnalysis();
        }
        return analysis;
    }

    /**
     * Parse to effective information.
     *
     * @param node
     * @param root
     */
    public List<String> parse(String theme, String text, boolean isPrint, Type type) {

        // 1. Get lines.
        List<String> lines = processToLines(text);

        // 2. Process to tree.
        List<String> allResult = new ArrayList<>();
        for (int i = 0; i < lines.size(); i++) {
            String[] content = WordBreaker.getInstance().splitToArray(lines.get(i));
            if (isPrint) {
                System.out.println(String.join(" ", content));
            }
            String desc = lines.get(i);
            if (desc.length() < 20) {
                if (i > 0) {
                    desc = lines.get(i - 1) + desc;
                } else if (lines.size() != 1) {
                    desc = desc + lines.get(1);
                }
            }
            List<String> temp = processTree(theme, desc, content, true, type);
            if (temp.size() > 0) {
                allResult.addAll(temp);
            }
        }
        return allResult;
    }

    /**
     * Process content to tree.
     *
     * @param content
     */
    private List<String> processTree(String theme, String desc, String[] content, boolean isPrint, Type type) {
        List<String> allResult = new ArrayList<>();
        List<CoreLabel> rawWords = Sentence.toCoreLabelList(content);
        Tree root = parser.apply(rawWords);
        List<Tree> ipRoots = Common.getMutilRoot(root);
        if (ipRoots.isEmpty()) {
            ipRoots.add(root.children()[0]);
        }

        for (Tree ipRoot : ipRoots) {
            for (int i = 0; i < formats.size(); i++) {
                IFormat format = formats.get(i);
                if (format.fName == type.name() || type == Type.ALL) {
                    if (format.parse(ipRoot, ipRoot)) {
                        List<String> result = format.getSentence(theme, desc);
                        if (isPrint) {
                            for (String res : result) {
                                System.out.println(res);
                            }
                        }
                        format.clear();
                        allResult.addAll(result);
                    }
                }
            }
        }
        return allResult;
    }

    /**
     * Process text to lines.
     *
     * @param text
     * @return
     */
    private List<String> processToLines(String text) {
        String[] lines = text.split("\\.|。|\\?|！|？|!|\\t|\\r|\\n|\\^");
        List<String> result = new ArrayList<>();
        for (String line : lines) {
            if (line.length() > 5) {
                result.add(line);
            }
        }
        return result;
    }
}
