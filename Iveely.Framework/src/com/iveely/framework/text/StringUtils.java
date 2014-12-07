package com.iveely.framework.text;

import java.io.UnsupportedEncodingException;

/**
 * String utils.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-12 21:39:51
 */
public class StringUtils {

    /**
     * Gets a string according to byte [].
     *
     * @param bytes
     * @return
     */
    public static String getString(byte[] bytes) {
        try {
            return new String(bytes, "UTF-8").trim();
        } catch (UnsupportedEncodingException ex) {
            return new String(bytes).trim();
        }
    }

    /**
     * Gets byte[] according to string.
     *
     * @param content
     * @return
     */
    public static byte[] getBytes(String content) {
        byte[] bytes = null;
        try {
            bytes = content.getBytes("UTF-8");
        } catch (UnsupportedEncodingException ex) {
            bytes = content.getBytes();
        }
        return bytes;
    }
}
