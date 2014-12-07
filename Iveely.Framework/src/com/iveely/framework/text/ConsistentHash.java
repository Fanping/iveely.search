package com.iveely.framework.text;

import java.util.List;
import java.util.SortedMap;
import java.util.TreeMap;

/**
 * Consistent hashing algorithm.
 *
 * @author liufanping@iveely.com
 * @param <T>
 * @date 2014-11-11 21:20:29
 */
public final class ConsistentHash<T> {

    /**
     * Hash calculation object, used to customize the hash algorithm.
     */
    HashFunc hashFunc;

    /**
     * Copy number of nodes.
     */
    private final int numberOfReplicas;

    /**
     * Consistency Hash ring.
     */
    private final SortedMap<Integer, T> circle = new TreeMap<>();

    public ConsistentHash(int numberOfReplicas, List<T> nodes) {
        this.numberOfReplicas = numberOfReplicas;
        this.hashFunc = (Object key) -> {
            String data = key.toString();
            final int p = 16777619;
            int hash = (int) 2166136261L;
            for (int i = 0; i < data.length(); i++) {
                hash = (hash ^ data.charAt(i)) * p;
            }
            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;
            return hash;
        };
        nodes.stream().forEach((node) -> {
            add(node);
        });
    }

    public ConsistentHash(HashFunc hashFunc, int numberOfReplicas, List<T> nodes) {
        this.numberOfReplicas = numberOfReplicas;
        this.hashFunc = hashFunc;
        nodes.stream().forEach((node) -> {
            add(node);
        });
    }

    /**
     * Increase node <br>
     * For each additional node, it will increase the number of nodes in a given
     * copy the closed loop.
     *
     * @param node
     */
    public void add(T node) {
        for (int i = 0; i < numberOfReplicas; i++) {
            circle.put(hashFunc.hash(node.toString() + i), node);
        }
    }

    /**
     * Remove nodes simultaneously removes the corresponding virtual node.
     *
     * @param node
     */
    public void remove(T node) {
        for (int i = 0; i < numberOfReplicas; i++) {
            circle.remove(hashFunc.hash(node.toString() + i));
        }
    }

    /**
     * To obtain a closest node clockwise.
     *
     * @param key
     * @return
     */
    public T get(Object key) {
        if (circle.isEmpty()) {
            return null;
        }
        int hash = hashFunc.hash(key);
        if (!circle.containsKey(hash)) {
            SortedMap<Integer, T> tailMap = circle.tailMap(hash);
            hash = tailMap.isEmpty() ? circle.firstKey() : tailMap.firstKey();
        }
        return circle.get(hash);
    }

    /**
     * Hash algorithm object, used to customize the hash algorithm.
     *
     */
    public interface HashFunc {

        public Integer hash(Object key);
    }
}
