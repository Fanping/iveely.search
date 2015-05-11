package com.iveely.database.type;

import com.iveely.database.common.Common;
import java.io.Serializable;

/**
 * Image as incode content
 *
 * @author liufanping@iveely.com
 * @date 2014-11-22 21:06:47
 */
public class Base64Image implements Serializable{

    /**
     * Base64 image content.
     */
    private String content;

    /**
     * @return the content
     */
    public String getContent() {
        return content;
    }

    public Base64Image(String base64) {
        if (base64.length() < Common.getBase64StringSize()) {
            this.content = base64;
        }
    }

    /**
     * @param base64 the content to set
     * @return 
     */
    public boolean setBase64(String base64) {
        if (base64.length() > Common.getBase64StringSize()) {
            return false;
        }
        this.content = base64;
        return true;
    }

    @Override
    public String toString() {
        if (this.content != null) {
            return this.content;
        }
        return "";
    }
}
