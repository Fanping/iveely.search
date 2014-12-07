package com.iveely.framework.net.cache;

import com.iveely.framework.text.StringUtils;
import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;

/**
 * Memory of cache.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-11 22:37:30
 */
public class Memory {

    /**
     * Single instance.
     */
    private static Memory memory;

    /**
     * Cache.
     */
    private static CacheMaster cacher;

    /**
     * Is cache init.
     */
    private boolean isCacheInit;

    private Memory() {
        isCacheInit = false;
    }

    public static Memory getInstance() {
        if (memory == null) {
            memory = new Memory();
        }
        return memory;
    }

    /**
     * Init cache for local machine.
     *
     * @param clientPath
     * @param useThread
     */
    public void initCache(String clientPath, boolean useThread) {
        if (!isCacheInit) {
            cacher = new CacheMaster(clientPath);
            if (useThread) {
                Thread thread = new Thread(cacher);
                thread.start();
            }
            isCacheInit = true;
        }
    }

    /**
     * Set value .
     *
     * @param key
     * @param data
     */
    public void set(String key, String data) {
        if (isCacheInit) {
            String ipAddress = cacher.getServer(key);
            String[] info = ipAddress.split(":");
            Client client = new Client(info[0], Integer.parseInt(info[1]));
            InternetPacket packet = new InternetPacket();
            packet.setData(StringUtils.getBytes(key + "#k-v#" + data));
            packet.setExecutType(17);
            packet.setMimeType(2);
            client.send(packet);
        }
    }

    /**
     * Get the store address which the key is specify.
     *
     * @param key
     * @return
     */
    public String getStoreAddress(String key) {
        if (isCacheInit) {
            return cacher.getServer(key);
        }
        return null;
    }

    /**
     * Append key.
     *
     * @param key
     * @param data
     */
    public void append(String key, String data) {
        if (isCacheInit) {
            String ipAddress = cacher.getServer(key);
            String[] info = ipAddress.split(":");
            Client client = new Client(info[0], Integer.parseInt(info[1]));
            InternetPacket packet = new InternetPacket();
            packet.setData(StringUtils.getBytes(key + "#k-v#" + data));
            packet.setExecutType(19);
            packet.setMimeType(2);
            client.send(packet);
        }
    }

    /**
     * Get value by key.
     *
     * @param key
     * @return
     */
    public String get(String key) {
        String val = MemoryObserver.getInstance().getValue(key);
        if (val != null) {
            return val;
        }
        if (isCacheInit) {
            String ipAddress = cacher.getServer(key);
            String[] info = ipAddress.split(":");
            if (info.length == 2) {
                Client client = new Client(info[0], Integer.parseInt(info[1]));
                InternetPacket packet = new InternetPacket();
                packet.setData(StringUtils.getBytes(key));
                packet.setExecutType(21);
                packet.setMimeType(2);
                packet = client.send(packet);
                //TODO: need to check the pack is vaild.
                return StringUtils.getString(packet.getData());
            }
        }
        return null;
    }

    /**
     * Register key.
     *
     * @param key
     */
    public void register(String key) {
        MemoryObserver.getInstance().register(key);
    }
}
