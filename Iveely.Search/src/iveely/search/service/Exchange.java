package iveely.search.service;

import java.io.UnsupportedEncodingException;

/**
 * Message transformation convention.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-1 18:21:58
 */
public class Exchange {

    /**
     * Execute type.
     */
    enum ExecuteType {

        // Text search.
        SEARCH,
        // Response search.
        RESP_SEARCH,
        // Add score.
        ADDSCORE,
        // Response add score.
        RESP_ADDSCORE,
        // Request snapshot.
        SNAPSHOT,
        // Response request snaphost.
        RESP_SNAPSHOT,
        // Image search.
        IMAGE,
        // Response image search.
        RESP_IMAGE
    }

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
     * Gets a byte [] according to string.
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
