/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.database.common;

import java.io.UnsupportedEncodingException;

/**
 *
 * @author X1 Carbon
 */
public class Message {

    /**
     * Convert string to byte[].
     *
     * @param content
     * @return
     */
    public static byte[] getBytes(String content) {
        byte[] bytes;
        try {
            bytes = content.getBytes("UTF-8");
        } catch (UnsupportedEncodingException ex) {
            bytes = content.getBytes();
        }
        return bytes;
    }

    /**
     * Convert byte[] to string.
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
}
