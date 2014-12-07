package iveely.search.store;

/**
 * Knowledge index.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-6 16:29:53
 */
public class KnowledgeIndex {

    /**
     * Term
     */
    private int term;

    /**
     * @return the term
     */
    public int getTerm() {
        return term;
    }

    /**
     * @param term the term to set
     */
    public void setTerm(int term) {
        this.term = term;
    }

    /**
     * Only 1,2,3
     */
    private int location;

    /**
     * @return the location
     */
    public int getLocation() {
        return location;
    }

    /**
     * @param location the location to set
     */
    public void setLocation(int location) {
        this.location = location;
    }

    private int knowledgeId;

    /**
     * @return the knowledgeId
     */
    public int getKnowledgeId() {
        return knowledgeId;
    }

    /**
     * @param knowledgeId the knowledgeId to set
     */
    public void setKnowledgeId(int knowledgeId) {
        this.knowledgeId = knowledgeId;
    }

    /**
     * Rank on this knowledge base.
     */
    private double rank;

    /**
     * @return the rank
     */
    public double getRank() {
        return rank;
    }

    /**
     * @param rank the rank to set
     */
    public void setRank(double rank) {
        this.rank = rank;
    }
}
