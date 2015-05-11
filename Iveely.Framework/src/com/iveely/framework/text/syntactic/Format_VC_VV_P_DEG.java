package com.iveely.framework.text.syntactic;

import edu.stanford.nlp.trees.Tree;
import java.util.ArrayList;
import java.util.List;

/**
 * VC_DEG. <br/>
 * 1. 复兴/NN 航空/NN 是/VC 总部/NN 位/VV 于/P 中国/NR 台湾/NR 的/DEG 一/CD 家/M 航空/NN 公司/NN.
 * <br/>
 * 2. 诺基亚/NR 公司/NN 是/VC 总部/NN 位于/VV 芬兰/NR 的/DEC 通信/NN 公司/NN.
 *
 * @author liufanping@iveely.com
 * @date 2015-2-14 14:07:23
 */
public class Format_VC_VV_P_DEG extends IFormat {

    public Format_VC_VV_P_DEG() {
        this.question = new ArrayList<>();
        this.answer = new ArrayList<>();
        this.fullAnswer = new ArrayList<>();
        this.answerTypes = new ArrayList<>();
        this.formatId = 3;
        this.fName = "Format_VC_VV_P_DEG";
    }

    @Override
    public boolean parse(Tree node, Tree ipRoot) {
        try {
            // 1. Check is NP to VP.
            if (!Common.isNP2VP(ipRoot)) {
                return false;
            }

            // 2. Check NP only contains NN.
            String name = "";
            Tree npNode = ipRoot.children()[0];
            if (npNode.children().length == 2) {
                String cTagNameA = npNode.getChild(0).value();
                String cTagNameB = npNode.getChild(1).value();
                if (cTagNameA.equals("NP") && cTagNameB.equals("PRN")) {
                    npNode = npNode.getChild(0);
                }
            } else if (npNode.children().length == 1) {

            } else {
                return false;
            }
            List<Tree> npChildren = npNode.getLeaves();
            for (int i = 0; i < npChildren.size(); i++) {
                String tag = npChildren.get(i).parent(ipRoot).value();
                if (tag.equals("NN")
                        || tag.equals("NR")
                        || tag.equals("NZ")) {
                    name += npChildren.get(i).value();
                } else {
                    return false;
                }
            }
            if (name.isEmpty()) {
                return false;
            }

            // 3.Check location.
            String question = name;
            Tree vpNode = ipRoot.lastChild();
            List<Tree> vpChildren = vpNode.getLeaves();

            // 3.1 first should be VC
            String fTag = vpChildren.get(0).parent(ipRoot).value();
            if (!fTag.equals("VC")) {
                return false;
            }
            question += vpChildren.get(0).value();

            // 3.2 find VV-P-*-DEG
            String lastTag = "";
            String fullAnswer = "";
            int stopFlag = 0;
            for (int i = 1; i < vpChildren.size(); i++) {
                String tag = vpChildren.get(i).parent(ipRoot).value();
                if ((tag.equals("P") && lastTag.equals("VV")) || (tag.equals("VV") && vpChildren.get(i).value().equals("位于"))) {
                    //question += vpChildren.get(i).value();
                    //fullAnswer += vpChildren.get(i).value();
                    stopFlag = i + 1;
                    //break;
                }
                if ((tag.equals("NR") || (tag.equals("NZ"))) && lastTag.equals("VV")) {
                    stopFlag = i;
                    break;
                }
                lastTag = tag;
                question += vpChildren.get(i).value();
                fullAnswer += vpChildren.get(i).value();
            }
            if (stopFlag == 0) {
                return false;
            }

            // 3.3 find DEG
            String location = "";
            int endIndex = 0;
            for (int i = stopFlag; i < vpChildren.size(); i++) {
                String tag = vpChildren.get(i).parent(ipRoot).value();
                if (tag.equals("NR") || tag.equals("NZ") || tag.equals("NN") || tag.equals("CD") || tag.equals("LC")) {
                    location += vpChildren.get(i).value();
                } else if (tag.equals("DEG") || tag.equals("DEC")) {
                    endIndex = i + 1;
                    question += "哪里";
                    question += vpChildren.get(i).value();
                    break;
                } else {
                    return false;
                }
            }
            if (location.isEmpty()) {
                return false;
            }

            // 4. Build question.
            for (int i = endIndex; i < vpChildren.size(); i++) {
                String tag = vpChildren.get(i).parent(ipRoot).value();
                if (tag.equals("PU")) {
                    break;
                }
                question += vpChildren.get(i).value();
            }
            question += "?";
            fullAnswer += location;
            this.question.add(question);
            this.answer.add(location);
            this.fullAnswer.add(fullAnswer);
            this.answerTypes.add(AnswerType.LOCATION);
            return true;
        } catch (Exception e) {
            e.printStackTrace();
        }
        return false;

    }

    @Override
    public String toString() {
        return "*-VC-*-VV-P-*-DEG-*";
    }

}
