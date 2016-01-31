package com.iveely.robot.util;

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
     * @param bytesSize
     * @return
     */
    public static byte[] int2byte(int intValue, int bytesSize) {
        byte[] targets = new byte[bytesSize];
        targets[0] = (byte) (intValue & 0xff);
        targets[1] = (byte) ((intValue >> 8) & 0xff);
        targets[2] = (byte) ((intValue >> 16) & 0xff);
        targets[3] = (byte) (intValue >>> 24);
        return targets;
    }
}
