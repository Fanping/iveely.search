package com.iveely.framework.text.syntactic;

import edu.stanford.nlp.trees.Tree;
import java.util.ArrayList;
import java.util.List;

/**
 * Synatactic check.
 *
 * @author liufanping@iveely.com
 * @date 2015-2-14 14:17:29
 */
public class Common {

    /**
     * Is np to vp sentence.
     *
     * @param ipRoot
     * @return
     */
    public static boolean isNP2VP(Tree ipRoot) {
        return checkNP2VP(ipRoot, ipRoot);
    }

    public static List<Tree> getMutilRoot(Tree root) {
        List<Tree> list = new ArrayList<>();
        if (root.children().length == 1 && "IP".equals(root.children()[0].value())) {
            Tree[] chTrees = root.children()[0].children();
            for (int i = 0; i < chTrees.length; i++) {
                if (chTrees[i].value().equals("IP")) {
                    list.add(chTrees[i]);
                }
            }
            return list;
        }
        return list;
    }

    /**
     * Check NP and VP.
     *
     * @param node
     * @return
     */
    private static boolean checkNP2VP(Tree node, Tree root) {
        Tree[] nodes = node.children();
        boolean isNP = false;
        boolean isVP = false;
        for (int i = 0; i < nodes.length; i++) {
            if (nodes[i].value().equals("NP")) {
                isNP = !isNP && checkNP(nodes[i], root);
            } else if (nodes[i].value().equals("VP")) {
                isVP = !isVP && checkVP(nodes[i], root);
            } else if (nodes[i].value().equals("PU")) {
                continue;
            } else {
                return false;
            }
        }
        return isNP && isVP;
    }

    /**
     * 1. NP can not contains 'PN'
     *
     * @param node
     * @return
     */
    private static boolean checkNP(Tree node, Tree root) {
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
    private static boolean checkVP(Tree node, Tree root) {
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

}
