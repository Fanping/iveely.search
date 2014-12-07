package com.iveely.framework.database.type;

/**
 * Short string.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 21:47:33
 */
public class ShortString {

    /**
     * The context.
     */
    private String data;

    /**
     * Set value.
     *
     * @param data
     * @return
     */
    public boolean setValue(String data) {
        if (data.length() < Common.getShortStringSize()) {
            this.data = data;
            return true;
        } else {
            this.data = "";
        }
        return false;
    }

    /**
     * Get value.
     *
     * @return
     */
    public String getValue() {
        return data;
    }

    public ShortString(String data) throws Exception {
        if (data.length() < Common.getShortStringSize()) {
            this.data = data;
        } else {
            throw new Exception(data + " is too long, max size is " + Common.getShortStringSize());
        }
    }

    /**
     * Get default short string.
     *
     * @return
     */
    public static ShortString getDefaultShortString() {
        try {
            ShortString val = new ShortString(" ");
            return val;
        } catch (Exception e) {
            // Never happen here.
        }
        return null;
    }

    @Override
    public String toString() {
        return data;
    }
}
