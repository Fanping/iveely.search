package com.iveely.framework.text.syntactic;

import edu.stanford.nlp.trees.Tree;
import java.util.ArrayList;
import java.util.List;

/**
 * abstract for Format.
 *
 * @author liufanping@iveely.com
 * @date 2015-2-13 22:31:37
 */
public abstract class IFormat {

    public String fName;
    
    /**
     * Question sentence.
     */
    protected List<String> question;

    /**
     * Answser sentence.
     */
    protected List<String> answer;

    /**
     * Full Answer.
     */
    protected List<String> fullAnswer;

    /**
     * All answers' type.
     */
    protected List<AnswerType> answerTypes;

    /**
     * Format Id.
     */
    protected int formatId;

    /**
     * Parse tree.
     *
     * @param node
     * @param root
     * @return
     */
    public abstract boolean parse(Tree node, Tree root);

    /**
     * Convert syntactic to an question.
     *
     * @return
     */
    public List<String> getQuestion() {
        return this.question;
    }

    /**
     * Get answer.
     *
     * @return
     */
    public List<String> getAnswer() {
        return this.answer;
    }

    /**
     * Get full answer.
     *
     * @return
     */
    public List<String> getFullAnswer() {
        return this.fullAnswer;
    }

    /**
     * Get all anwser types.
     *
     * @return
     */
    public List<AnswerType> getAnswerTypes() {
        return this.answerTypes;
    }

    /**
     * Get total count.
     *
     * @return
     */
    public int getTotalCount() {
        return this.question.size();
    }

    /**
     * Get all question sentence.
     *
     * @return
     */
    public List<String> getSentence(String theme, String desc) {
        List<String> list = new ArrayList<>();
        for (int i = 0; i < getTotalCount(); i++) {
            String temp = "";
            temp = formatId + "|" + theme + "|" + this.question.get(i) + "|" + this.answer.get(i) + "|" + desc + "|" + this.answerTypes.get(i).toString();
            list.add(temp);
        }
        return list;
    }

    /**
     * Clear records.
     */
    public void clear() {
        this.question.clear();
        this.answer.clear();
        this.fullAnswer.clear();
        this.answerTypes.clear();
    }
}
