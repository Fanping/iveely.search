package com.iveely.framework.text;

import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;

/**
 * Ring buffer.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-11 21:46:49
 */
public class CyclingBuffer {

    /**
     * Buffer capacity.
     */
    private int capacity;

    /**
     * Last updated index bits.
     */
    private int latestIndex;

    /**
     * Buffer pool.
     */
    private Object[] buffer;

    /**
     * Quick search.
     */
    private TreeMap<String, Integer> map;

    public CyclingBuffer(int size) {
        if (size > 0) {
            this.capacity = size;
            buffer = new Object[capacity];
            map = new TreeMap<>();
        }
    }

    /**
     * Update the value of the ring buffer zone.
     *
     * @param key
     * @param value
     */
    public void add(String key, Object value) {
        int cIndex = (latestIndex++ % capacity);
        buffer[cIndex] = value;
        latestIndex %= capacity;
        map.put(key, cIndex);
    }

    public Object read(String key) {
        if (map.containsKey(key)) {
            int cIndex = map.get(key);
            return buffer[cIndex];
        }
        return null;
    }

    /**
     * Get a ring buffer current data (FIFO).
     *
     * @return
     */
    public Object getCurrentData() {
        return latestIndex > 0 ? buffer[(latestIndex - 1) % capacity] : buffer[capacity - 1];
    }

    /**
     * Get the ring buffer all data (last out).
     *
     * @return
     */
    public Object[] read() {
        List<Object> avaiableData = new ArrayList<>();
        for (int i = latestIndex % capacity; i > -1; i--) {
            avaiableData.add(buffer[i]);
        }
        for (int i = capacity - 1; i > latestIndex % capacity; i--) {
            avaiableData.add(buffer[i]);
        }
        return avaiableData.toArray();
    }

    /**
     * If certain a keyword.
     *
     * @param key
     * @return
     */
    public boolean isContains(String key) {
        return map.containsKey(key);
    }
}
