package iveely.search.store;

import com.iveely.framework.database.type.ShortString;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Index of wikipedia.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-5 20:50:21
 */
public class WikipediaLocation {

    /**
     * term of wikipedia.
     */
    private ShortString term;

    /**
     * @return the term
     */
    public String getTerm() {
        if (term != null) {
            return term.getValue();
        }
        return "";
    }

    /**
     * @param term the term to set
     */
    public boolean setTerm(String term) {
        try {
            this.term = new ShortString(term);
            return true;
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * Store id.
     */
    private Integer storeId;

    /**
     * @return the storeId
     */
    public Integer getStoreId() {
        return storeId;
    }

    /**
     * @param storeId the storeId to set
     */
    public void setStoreId(Integer storeId) {
        this.storeId = storeId;
    }
}
