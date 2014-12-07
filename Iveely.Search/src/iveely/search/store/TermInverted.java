package iveely.search.store;

import java.util.Objects;

/**
 *
 * Inverted index of text search.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-4 19:43:59
 */
public class TermInverted implements Comparable {

    /**
     * Term.
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
     * Page id.
     */
    private int page;

    /**
     * @return the page
     */
    public int getPage() {
        return page;
    }

    /**
     * @param page the page to set
     */
    public void setPage(int page) {
        this.page = page;
    }

    /**
     * Rank.
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

    @Override
    public int compareTo(Object o) {
        TermInverted info = (TermInverted) o;
        if (info.getPage() == this.getPage() && !Objects.equals(o, this)) {
            info.setRank((this.rank + info.getRank()) * 1.2);
            return 0;
        }
        if (info.getRank() > this.rank) {
            return 1;
        }
        return -1;
    }
}
