package com.iveely.framework.text.syntactic;

import edu.stanford.nlp.trees.Tree;
import java.util.ArrayList;
import java.util.List;

/**
 * NR-NT.<br/>
 * 1. 亚历山大·贝尔（1847年3月3日－1922年8月2日） <br/>
 * 2. 贝尔/NR（/PU 1898年/NT 11月/NT 22日/NT —/PU 1948年/NT 8月/NT 12日/NT ）/PU <br/>
 *
 * @author liufanping@iveely.com
 * @date 2015-2-14 12:09:43
 */
public class Format_NR_NT extends IFormat {

    public Format_NR_NT() {
        this.question = new ArrayList<>();
        this.answer = new ArrayList<>();
        this.fullAnswer = new ArrayList<>();
        this.answerTypes = new ArrayList<>();
        this.formatId = 2;
        this.fName = "Format_NR_NT";
    }

    /**
     * Pu index after the fName;
     */
    private int puIndex;

    @Override
    public boolean parse(Tree node, Tree root) {
        try {
            List<Tree> lefts = node.getLeaves();
            if (lefts.size() < 5) {
                return false;
            }

            // 1. First must be NR.
            puIndex = 0;
            String name = findName(lefts, root);
            if (name.isEmpty()) {
                return false;
            }

            // 2. Second must be PU.
            puIndex++;
            String secondTag = lefts.get(puIndex).parent(root).value();
            if (!secondTag.equals("PU")) {
                return false;
            }

            // 3. Find the next PU.
            String dateA = "";
            int j = puIndex + 1;
            for (int i = puIndex + 1; i < lefts.size(); i++, j++) {
                String parentTag = lefts.get(i).parent(root).value();
                if (parentTag.equals("NT")) {
                    dateA += lefts.get(i).value();
                } else if (parentTag.equals("PU")) {
                    break;
                } else {
                    return false;
                }
            }

            // 4. Find the next PU.
            String dateB = "";
            for (int i = j + 1; i < lefts.size(); i++) {
                String parentTag = lefts.get(i).parent(root).value();
                if (parentTag.equals("NT")) {
                    dateB += lefts.get(i).value();
                } else if (parentTag.equals("PU")) {
                    break;
                }
            }

            if (name.isEmpty() || dateA.isEmpty() || dateB.isEmpty()) {
                return false;
            }

            // 4.1 build answer.
            this.question.add(name + "生于哪一年?");
            this.answer.add(dateA);
            this.fullAnswer.add("生于" + dateA);
            this.answerTypes.add(AnswerType.DATE);

            this.question.add(name + "逝世于哪一年?");
            this.answer.add(dateB);
            this.fullAnswer.add("逝世于" + dateA);
            this.answerTypes.add(AnswerType.DATE);
            return true;
        } catch (Exception e) {
            e.printStackTrace();
        }
        return false;
    }

    /**
     * Find fName.
     *
     * @param list
     * @return
     */
    private String findName(List<Tree> list, Tree root) {
        String name = "";
        for (int i = 0; i < list.size(); i++) {
            String tag = list.get(i).parent(root).value();
            if (tag.equals("NZ") || tag.equals("NR") || (tag.equals("PU") && list.get(i).value().equals("·"))) {
                name += list.get(i).value();
                puIndex = i;
            } else {
                return name;
            }
        }
        return name;
    }

    @Override
    public String toString() {
        return "*-NR-PU-NT-*";
    }

}
