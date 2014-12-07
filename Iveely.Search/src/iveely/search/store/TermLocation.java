package iveely.search.store;

/**
 * Term location index.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-4 19:36:50
 */
public class TermLocation {

    /**
     * Term.
     */
    private Integer term;

    /**
     * @return the term
     */
    public Integer getTerm() {
        return term;
    }

    /**
     * @param term the term to set
     */
    public void setTerm(Integer term) {
        this.term = term;
    }

    /**
     * Start postion.
     */
    private int startPostion;

    /**
     * @return the startPostion
     */
    public int getStartPostion() {
        return startPostion;
    }

    /**
     * @param startPostion the startPostion to set
     */
    public void setStartPostion(int startPostion) {
        this.startPostion = startPostion;
    }

    /**
     * End postion.
     */
    private int endPostion;

    /**
     * @return the endPostion
     */
    public int getEndPostion() {
        return endPostion;
    }

    /**
     * @param endPostion the endPostion to set
     */
    public void setEndPostion(int endPostion) {
        this.endPostion = endPostion;
    }
}
