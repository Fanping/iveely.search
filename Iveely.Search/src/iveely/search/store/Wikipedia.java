package iveely.search.store;

import com.iveely.framework.database.type.ShortString;

/**
 * Wikipedia.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-5 20:36:36
 */
public class Wikipedia {

    /**
     * Url.
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
     * @return
     */
    public boolean setUrl(String url) {
        try {
            this.url = new ShortString(url);
            return true;
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * Theme of wiki.D
     */
    private ShortString theme;

    /**
     * @return the theme
     */
    public String getTheme() {
        if (theme != null) {
            return theme.getValue();
        }
        return "";
    }

    /**
     * @param theme the theme to set
     * @return
     */
    public boolean setTheme(String theme) {
        try {
            this.theme = new ShortString(theme);
            return true;
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * Abstract context.
     */
    private String absArticle;

    /**
     * @return the absArticle
     */
    public String getAbsArticle() {
        return absArticle;
    }

    /**
     * @param absArticle the absArticle to set
     */
    public void setAbsArticle(String absArticle) {
        this.absArticle = absArticle;
    }
}
