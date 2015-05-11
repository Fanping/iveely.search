package com.iveely.framework.net.cache;

import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.net.Server;
import com.iveely.framework.net.Internet;
import com.iveely.framework.text.StringUtils;
import java.util.TreeMap;

/**
 * Distribute Cache Observer.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-12 21:24:06
 */
public class MemoryObserver implements Runnable {

    /**
     * The same memory from cache client.
     */
    private final TreeMap<String, String> data;

    /**
     * The listener of the key event.
     */
    private final MemoryEvent eventListener;

    /**
     * The call back server.
     */
    private final Server server;

    /**
     * The single instance.
     */
    private static MemoryObserver observer;

    private MemoryObserver(int callBackPort) {
        data = new TreeMap<>();
        eventListener = new MemoryEvent(this);
        server = new Server(eventListener, callBackPort);
    }

    /**
     * Get single instance in system.
     *
     * @return
     */
    public static MemoryObserver getInstance() {
        if (observer == null) {
            int port = Internet.getAvaliablePort(8001);
            if (port != -1) {
                observer = new MemoryObserver(port);
            }
        }
        return observer;
    }

    /**
     * Register key to server.
     *
     * @param key
     * @return
     */
    public boolean register(String key) {
        if (data.containsKey(key)) {
            return false;
        }
        String localIp = Internet.getLocalIpAddress();
        if (localIp == null) {
            return false;
        }
        String address = Memory.getInstance().getStoreAddress(key);
        //TODO:If the server be dropped. please regist to anthor client.
        String[] info = address.split(":");
        Client client = new Client(info[0], Integer.parseInt(info[1]));
        InternetPacket packet = new InternetPacket();
        packet.setExecutType(23);
        packet.setMimeType(2);
        //TODO:Set the real ip & port.
        packet.setData(StringUtils.getBytes(key + "_" + localIp + ":" + server.getPort()));
        packet = client.send(packet);
        return packet != null && packet.getExecutType() != 999;
    }

    /**
     * The key changed event.
     *
     * @param key
     * @param val
     */
    public void onKeyEvent(String key, String val) {
        data.put(key, val);
    }

    /**
     * Get the value by key.
     *
     * @param key
     * @return
     */
    public String getValue(String key) {
        if(data==null){
            return null;
        }
        return data.get(key);
    }

    @Override
    public void run() {
        server.start();
    }
}
