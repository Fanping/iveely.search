package com.iveely.framework.text.syntactic;

import edu.stanford.nlp.trees.Tree;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;

/**
 * NR-NNT. <br/>
 * 1. 张亚勤/NR 博士/NNT <br/>
 * 2. 阿里巴巴/NR 创始人/NNT 马云/NR <br/>
 * 3. 中国/NR 企业家/NNT <br/>
 * 4. 中国/NR 企业家/NNT 马云/NR
 *
 * @author liufanping@iveely.com
 * @date 2015-2-13 22:32:24
 */
public class Format_NR_NNT extends IFormat {

    public Format_NR_NNT() {
        this.stopWords = new HashSet<>();
        stopWords.add("中国");
        stopWords.add("美国");

        this.question = new ArrayList<>();
        this.answer = new ArrayList<>();
        this.fullAnswer = new ArrayList<>();
        this.answerTypes = new ArrayList<>();

        this.formatId = 1;
        this.fName = "Format_NR_NNT";
    }

    /**
     * Stop words.
     */
    private HashSet<String> stopWords;

    @Override
    public boolean parse(Tree node, Tree root) {
        try {
            List<Tree> lefts = node.getLeaves();
            String lastTag = "";
            String lastWords = "";
            for (int i = 0; i < lefts.size(); i++) {
                String val = lefts.get(i).parent(root).value();
                if (val.equals("NNT") && lastTag.equals("NR")) {
                    if (i < lefts.size() - 1) {
                        String nextVal = lefts.get(i + 1).parent(root).value();
                        if (nextVal.equals("NR")) {
                            if (!stopWords.contains(lastWords)) {
                                this.question.add(lastWords + "的" + lefts.get(i).value() + "是谁?");
                                String anwser = getValue(lefts, root, i + 1);
                                this.answer.add(anwser);
                                this.fullAnswer.add(anwser);
                                this.answerTypes.add(AnswerType.WHOM);
                                return true;
                            }
                            return false;
                        }
                    }
                    if (!stopWords.contains(lastWords)) {
                        this.question.add(lastWords + "是" + lefts.get(i).value() + "吗?");
                        this.answer.add("是");
                        this.fullAnswer.add("是" + lefts.get(i).value());
                        this.answerTypes.add(AnswerType.JUDGE);
                        return true;
                    }
                    return false;
                }
                if (!val.equals("NR")) {
                    return false;
                }
                lastTag = val;
                if ("NR".equals(lastTag)) {
                    lastWords += lefts.get(i).value();
                } else {
                    lastWords = lefts.get(i).value();
                }
            }
            return false;
        } catch (Exception e) {
            e.printStackTrace();
        }
        return false;
    }

    /**
     * Get continuous value.
     *
     * @param left
     * @param root
     * @param start
     * @return
     */
    private String getValue(List<Tree> left, Tree root, int start) {
        String val = left.get(start).value();
        for (int i = start + 1; i < left.size(); i++) {
            if ("NR".equals(left.get(i).parent(root).value())) {
                val += left.get(i).value();
            } else {
                return val;
            }
        }
        return val;
    }

    @Override
    public String toString() {
        return "*-NR-NNT-*";
    }
}
