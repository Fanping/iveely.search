package iveely.search.plugin.math;

/**
 * Algorithm helper.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 10:38:07
 */
public class ArithHelper {

    /**
     * The default precision division.
     */
    private static final int DEF_DIV_SCALE = 16;

    private ArithHelper() {
    }

    /**
     * Provide accurate addition.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double add(double v1, double v2) {
        java.math.BigDecimal b1 = new java.math.BigDecimal(Double.toString(v1));
        java.math.BigDecimal b2 = new java.math.BigDecimal(Double.toString(v2));
        return b1.add(b2).doubleValue();
    }

    /**
     * Provide accurate addition.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double add(String v1, String v2) {
        java.math.BigDecimal b1 = new java.math.BigDecimal(v1);
        java.math.BigDecimal b2 = new java.math.BigDecimal(v2);
        return b1.add(b2).doubleValue();
    }

    /**
     * Provide accurate subtraction.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double sub(double v1, double v2) {
        java.math.BigDecimal b1 = new java.math.BigDecimal(Double.toString(v1));
        java.math.BigDecimal b2 = new java.math.BigDecimal(Double.toString(v2));
        return b1.subtract(b2).doubleValue();
    }

    /**
     * Provide accurate subtraction.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double sub(String v1, String v2) {
        java.math.BigDecimal b1 = new java.math.BigDecimal(v1);
        java.math.BigDecimal b2 = new java.math.BigDecimal(v2);
        return b1.subtract(b2).doubleValue();
    }

    /**
     * Provides accurate multiplication.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double mul(double v1, double v2) {
        java.math.BigDecimal b1 = new java.math.BigDecimal(Double.toString(v1));
        java.math.BigDecimal b2 = new java.math.BigDecimal(Double.toString(v2));
        return b1.multiply(b2).doubleValue();
    }

    /**
     * Provides accurate multiplication.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double mul(String v1, String v2) {
        java.math.BigDecimal b1 = new java.math.BigDecimal(v1);
        java.math.BigDecimal b2 = new java.math.BigDecimal(v2);
        return b1.multiply(b2).doubleValue();
    }

    /**
     * Provide (relatively) precise division, except when the situation occurs
     * when the endless, accurate to 10 decimal point, after the figures are
     * rounded.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double div(double v1, double v2) {
        return div(v1, v2, DEF_DIV_SCALE);
    }

    /**
     * Provide (relatively) precise division, except when the situation occurs
     * when the endless, accurate to 10 decimal point, after the figures are
     * rounded.
     *
     * @param v1
     * @param v2
     * @return
     */
    public static double div(String v1, String v2) {
        java.math.BigDecimal b1 = new java.math.BigDecimal(v1);
        java.math.BigDecimal b2 = new java.math.BigDecimal(v2);
        return b1.divide(b2, DEF_DIV_SCALE, java.math.BigDecimal.ROUND_HALF_UP).doubleValue();
    }

    /**
     * Providing (relatively) accurate division. When occurrence except endless,
     * specify the scale parameter accuracy, after rounding numbers.
     *
     * @param v1
     * @param v2
     * @param scale。
     * @return
     */
    public static double div(double v1, double v2, int scale) {
        if (scale < 0) {
            throw new IllegalArgumentException("The   scale   must   be   a   positive   integer   or   zero");
        }
        java.math.BigDecimal b1 = new java.math.BigDecimal(Double.toString(v1));
        java.math.BigDecimal b2 = new java.math.BigDecimal(Double.toString(v2));
        return b1.divide(b2, scale, java.math.BigDecimal.ROUND_HALF_UP).doubleValue();
    }

    /**
     * Provides precise decimals rounded handle.
     *
     * @param v
     * @param scale
     * @return
     */
    public static double round(double v, int scale) {
        if (scale < 0) {
            throw new IllegalArgumentException("The   scale   must   be   a   positive   integer   or   zero");
        }
        java.math.BigDecimal b = new java.math.BigDecimal(Double.toString(v));
        java.math.BigDecimal one = new java.math.BigDecimal("1");
        return b.divide(one, scale, java.math.BigDecimal.ROUND_HALF_UP).doubleValue();
    }

    /**
     * Provides precise decimals rounded handle.
     *
     * @param v
     * @param scale
     * @return
     */
    public static double round(String v, int scale) {
        if (scale < 0) {
            throw new IllegalArgumentException("The   scale   must   be   a   positive   integer   or   zero");
        }
        java.math.BigDecimal b = new java.math.BigDecimal(v);
        java.math.BigDecimal one = new java.math.BigDecimal("1");
        return b.divide(one, scale, java.math.BigDecimal.ROUND_HALF_UP).doubleValue();
    }
}
