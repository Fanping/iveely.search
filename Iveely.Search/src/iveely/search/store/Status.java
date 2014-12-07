package iveely.search.store;

/**
 * Status of text search.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-21 22:52:59
 */
public class Status {

    /**
     * Id of the url which current read.
     */
    private int urlId;

    /**
     * @return the urlId
     */
    public int getUrlId() {
        return urlId;
    }

    /**
     * @param urlId the urlId to set
     */
    public void setUrlId(int urlId) {
        this.urlId = urlId;
    }

    /**
     * Current page id.
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
     * Last page Id.
     */
    private int lastPageId;

    /**
     * @return the lastPageId
     */
    public int getLastPageId() {
        return lastPageId;
    }

    /**
     * @param lastPageId the lastPageId to set
     */
    public void setLastPageId(int lastPageId) {
        this.lastPageId = lastPageId;
    }
}
