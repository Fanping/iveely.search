package com.iveely.framework.java;

/**
 *
 * @author liufanping@iveely.com
 * @param <T>
 * @date 2014-10-23 21:03:50
 */
public class RefObject<T> {

    public T argvalue;

    public RefObject(T refarg) {
        argvalue = refarg;
    }
}
