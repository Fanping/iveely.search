package com.iveely.framework.net.cache;

import com.iveely.framework.file.Reader;
import com.iveely.framework.text.ConsistentHash;
import java.io.File;
import java.util.ArrayList;
import java.util.List;

/**
 * Distribute cache master.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-11 22:12:05
 */
public final class CacheMaster implements Runnable {

    /**
     * Distributor
     */
    private final ConsistentHash<String> distributor;

    /**
     * Store house.
     */
    private final List<String> houses;

    /**
     * The path of the client store.
     */
    private final String clientPath;

    public CacheMaster(String clientPath) {
        this.clientPath = clientPath;
        houses = new ArrayList<>();
        distributor = new ConsistentHash<>(8, houses);
        updateHouses();
    }

    /**
     * Get server by key.
     *
     * @param key
     * @return
     */
    public String getServer(String key) {
        return distributor.get(key);
    }

    /**
     * Update house.
     */
    public void updateHouses() {
        String[] allClients = Reader.readToString(new File(this.clientPath), "utf-8").split(" ");
        if (allClients == null) {
            return;
        }
        for (String allClient : allClients) {
            if (!houses.contains(allClient)) {
                distributor.add(allClient);
            }
        }
        for (int j = 0; j < houses.size(); j++) {
            boolean isExist = false;
            for (String allClient : allClients) {
                if (houses.get(j).equals(allClient)) {
                    isExist = true;
                }
            }
            if (!isExist) {
                houses.remove(j);
            }
        }
    }

    @Override
    public void run() {
        if (MemoryObserver.getInstance() != null) {
            Thread observer = new Thread(MemoryObserver.getInstance());
            observer.start();
        }
        while (true) {
            try {
                Thread.sleep(1000 * 60);
                updateHouses();
            } catch (InterruptedException ex) {
            }
        }
    }
}
