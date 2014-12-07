package iveely.search.store;

import com.iveely.framework.database.type.Base64Image;
import com.iveely.framework.database.type.ShortString;
import iveely.search.service.SlaveService;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 * Image Info.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-7 23:31:54
 */
public class Image {

    /**
     * Where the document number.
     */
    private int Id;

    /**
     * @return the Id
     */
    public int getId() {
        return Id;
    }

    /**
     * @param Id the Id to set
     */
    public void setId(int Id) {
        this.Id = Id;
    }

    /**
     * Description.
     */
    private ShortString alt;

    /**
     * @return the alt
     */
    public String getAlt() {
        if (alt != null) {
            return alt.getValue();
        }
        return "";
    }

    /**
     * @param alt the alt to set
     */
    public void setAlt(String alt) {
        try {
            this.alt = new ShortString(alt);
        } catch (Exception ex) {
            Logger.getLogger(Image.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    /**
     * Image url.
     */
    private ShortString url;

    /**
     * @return the url
     */
    public String getUrl() {
        if (url != null) {
            return url.getValue();
        }
        return "";
    }

    /**
     * @param url the url to set
     */
    public void setUrl(String url) {
        try {
            this.url = new ShortString(url);
        } catch (Exception ex) {
            Logger.getLogger(Image.class.getName()).log(Level.SEVERE, null, ex);
        }
    }

    private ShortString pageUrl;

    /**
     * Image data.
     */
    private Base64Image image;

    /**
     * Is logo.
     */
    private boolean isLogo;

    /**
     * @return the isLogo
     */
    public boolean isIsLogo() {
        return isLogo;
    }

    /**
     * @param isLogo the isLogo to set
     */
    public void setIsLogo(boolean isLogo) {
        this.isLogo = isLogo;
    }

    /**
     * @return the image
     */
    public String getImage() {
        if (this.image != null) {
            return image.getContent();
        }
        return "";
    }

    /**
     * @param base64
     * @return
     */
    public boolean setImage(String base64) {
        this.image = new Base64Image();
        return this.image.setBase64(base64);
    }

    /**
     * @return the pageUrl
     */
    public String getPageUrl() {
        if (this.pageUrl == null) {
            return "";
        }
        return this.pageUrl.getValue();
    }

    /**
     * @param pageUrl the pageUrl to set
     * @return
     */
    public boolean setPageUrl(String pageUrl) {
        try {
            this.pageUrl = new ShortString(pageUrl);
        } catch (Exception ex) {
            return false;
        }
        return true;
    }

    /**
     * Image search summary.
     *
     * @param rank
     * @param query
     * @return
     */
    public String simple(double rank, String query) {
        StringBuilder buffer = new StringBuilder();
        buffer.append("[WEIGHT]:");
        buffer.append(rank);
        buffer.append("\n");
        buffer.append("[IMAGEID]:");
        buffer.append(System.currentTimeMillis());
        buffer.append("\n");
        buffer.append("[ALT]:");
        buffer.append(getAlt());
        buffer.append("\n");
        buffer.append("[URL]:");
        buffer.append(getPageUrl());
        buffer.append("\n");
        buffer.append("[IMAGEURL]:");
        buffer.append(getUrl());
        buffer.append("\n");
        buffer.append("[CONTENT]:");
        buffer.append(getImage());
        buffer.append("\n");
        buffer.append("[QUERY]:");
        buffer.append(query);
        buffer.append("\n");
        buffer.append("[FROMSERVER]:");
        buffer.append(SlaveService.getFlag());
        buffer.append("\n");
        return buffer.toString();
    }
}
