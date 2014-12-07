package iveely.search.store;

import com.iveely.framework.database.type.ShortString;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * User click data.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-6 22:28:03
 */
public class UserClick {

    /**
     * Page numbering.
     */
    private int pageId;

    /**
     * @return the pageId
     */
    public int getPageId() {
        return pageId;
    }

    /**
     * @param pageId the pageId to set
     */
    public void setPageId(int pageId) {
        this.pageId = pageId;
    }

    /**
     * Users search terms.
     */
    private ShortString query;

    /**
     * @return the query
     */
    public String getQuery() {
        if (query != null) {
            return query.getValue();
        }
        return "";
    }

    /**
     * @param query the query to set
     */
    public void setQuery(String query) {
        try {
            this.query = new ShortString(query);
        } catch (Exception ex) {
            Logger.getLogger(UserClick.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    /**
     * Search timestamp.
     */
    private long timestamp;

    /**
     * @return the timestamp
     */
    public long getTimestamp() {
        return timestamp;
    }

    /**
     * @param timestamp the timestamp to set
     */
    public void setTimestamp(long timestamp) {
        this.timestamp = timestamp;
    }
}
