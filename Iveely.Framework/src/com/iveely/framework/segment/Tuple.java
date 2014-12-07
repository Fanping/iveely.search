package com.iveely.framework.segment;

/**
 * Tuple.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 10:35:52
 */
class Tuple {

    /**
     * Key array.
     */
    private final String[] tArray;

    /**
     * Value array.
     */
    private final String[] vArray;

    public Tuple(String[] t, String[] v) {
        tArray = t;
        vArray = v;
    }

    public int getTLength() {
        if (tArray != null) {
            return tArray.length;
        }
        return -1;
    }

    public int getVLength() {
        if (vArray != null) {
            return vArray.length;
        }
        return -1;
    }

    public String getTStr(int i) {
        if (tArray != null && i < tArray.length) {
            return tArray[i];
        }
        return "";
    }

    public String getVStr(int i) {
        if (vArray != null && i < vArray.length) {
            return vArray[i];
        }
        return "";
    }

    public String[] getT() {
        return tArray;
    }

    public String[] getV() {
        return vArray;
    }
}
