package iveely.search.store;

/**
 * Knowledge Term location.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-6 16:28:50
 */
public class KTermLocation {

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
     * Starting position.
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
     * End position.
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
