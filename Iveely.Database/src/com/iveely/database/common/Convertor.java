package com.iveely.database.common;

import com.iveely.database.storage.Types;
import com.iveely.database.type.Base64Image;
import com.iveely.database.type.ShortString;

/**
 * Convertor for int and byte[4].
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 22:30:56
 */
public class Convertor {

    /**
     * Convert byte[4] to int.
     *
     * @param bytes
     * @return
     */
    public static int bytesToInt(byte[] bytes) {
        int targets = (bytes[0] & 0xff) | ((bytes[1] << 8) & 0xff00) | ((bytes[2] << 24) >>> 8) | (bytes[3] << 24);
        return targets;
    }

    /**
     * Convert int to byte[4];
     *
     * @param intValue
     * @return
     */
    public static byte[] int2byte(int intValue) {
        byte[] targets = new byte[4];
        targets[0] = (byte) (intValue & 0xff);
        targets[1] = (byte) ((intValue >> 8) & 0xff);
        targets[2] = (byte) ((intValue >> 16) & 0xff);
        targets[3] = (byte) (intValue >>> 24);
        return targets;
    }

    public static byte[] long2byte(long x) {
        byte[] targets = new byte[8];
        targets[7] = (byte) (x >> 56);
        targets[6] = (byte) (x >> 48);
        targets[5] = (byte) (x >> 40);
        targets[4] = (byte) (x >> 32);
        targets[3] = (byte) (x >> 24);
        targets[2] = (byte) (x >> 16);
        targets[1] = (byte) (x >> 8);
        targets[0] = (byte) (x);
        return targets;
    }

    public static long byte2long(byte[] bb) {
        return ((((long) bb[7] & 0xff) << 56)
                | (((long) bb[6] & 0xff) << 48)
                | (((long) bb[ 5] & 0xff) << 40)
                | (((long) bb[4] & 0xff) << 32)
                | (((long) bb[3] & 0xff) << 24)
                | (((long) bb[ 2] & 0xff) << 16)
                | (((long) bb[1] & 0xff) << 8) | (((long) bb[0] & 0xff)));
    }

    /**
     * Convert string to specify type.
     *
     * @param value
     * @param type
     * @return
     */
    public static Object string2Object(String value, Types type) {
        switch (type) {
            case INTEGER:
                return Integer.parseInt(value);
            case LONG:
                return Long.parseLong(value);
            case DOUBLE:
                return Double.parseDouble(value);
            case BOOLEAN:
                return Boolean.parseBoolean(value);
            case FLOAT:
                return Float.parseFloat(value);
            case CHAR:
                return value.toCharArray()[0];
            case SHORTSTRING:
                return new ShortString(value);
            case IMAGE:
                return new Base64Image(value);
            default:
                return value;
        }
    }
}
