package iveely.search.store;

import com.iveely.framework.database.type.ShortString;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Url entity.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-21 22:33:04
 */
public class Url {

    public Url() {
        timestamp = -1;
    }

    /**
     * Url data.
     */
    private ShortString url;

    /**
     * @return the url
     */
    public String getUrl() {
        if (this.url == null) {
            return "";
        }
        return this.url.getValue();
    }

    /**
     * @param url the url to set
     */
    public void setUrl(String url) {
        try {
            this.url = new ShortString(url);
        } catch (Exception ex) {
            Logger.getLogger(Url.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    /**
     * The link points to the parent link.
     */
    private int parentUrl;

    /**
     * @return the parentUrl
     */
    public int getParentUrl() {
        return parentUrl;
    }

    /**
     * @param parentUrl the parentUrl to set
     */
    public void setParentUrl(int parentUrl) {
        this.parentUrl = parentUrl;
    }

    /**
     * Server timestamp.
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
