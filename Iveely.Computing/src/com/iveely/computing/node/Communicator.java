package com.iveely.computing.node;

import com.iveely.computing.common.IStreamCallback;
import com.iveely.computing.common.StreamPacket;
import com.iveely.computing.status.SystemConfig;
import com.iveely.computing.zookeeper.ZookeeperClient;
import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.net.Server;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.UUID;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * Data communicator.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-6 21:41:22
 */
public class Communicator {

    /**
     * Slot of communicator.
     */
    public static class Slot implements Runnable, ICallback {

        /**
         * communicat task.
         */
        public class Task implements Runnable {

            /**
             * Callback of communicat.
             */
            private final IStreamCallback callback;

            /**
             * Stream packet.
             */
            private final StreamPacket packet;

            public Task(IStreamCallback callback, StreamPacket packet) {
                this.callback = callback;
                this.packet = packet;
            }

            @Override
            public void run() {
                this.callback.invoke(this.packet);
            }
        }

        /**
         * Service.
         */
        private final Server server;

        /**
         * Get name of the slot.
         */
        private final String name;

        /**
         * All stream callback.
         */
        private final HashMap<String, IStreamCallback> callbacks;

        /**
         * Thread pool.
         */
        private final ExecutorService threadPool;

        /**
         * Port of the slot.
         */
        private final int port;

        /**
         * Current in using count.
         */
        private int usingCount;

        public Slot(int port) {
            this.server = new Server(this, SystemConfig.slotBasePort + port);
            this.port = 6000 + port;
            this.name = UUID.randomUUID().toString();
            this.callbacks = new HashMap<>();
            this.threadPool = Executors.newFixedThreadPool(100);
            this.usingCount = 0;
        }

        /**
         * Register stream callback.
         *
         * @param guid
         * @param callback
         */
        public void register(String guid, IStreamCallback callback) {
            callbacks.put(guid, callback);
            this.usingCount++;
        }

        /**
         * Unregister stream callback.
         *
         * @param guid
         */
        public void unregister(String guid) {
            if (callbacks.containsKey(guid)) {
                callbacks.remove(guid);
                this.usingCount--;
            }
        }

        /**
         * Get count of slot in using.
         *
         * @return
         */
        public int getUsingCount() {
            return this.usingCount;
        }

        /**
         * Get connect string.
         *
         * @return
         */
        public String getConnectString() {
            return com.iveely.framework.net.Internet.getLocalIpAddress() + "," + this.port;
        }

        /**
         * Get name of this slot.
         *
         * @return
         */
        public String getName() {
            return this.name;
        }

        @Override
        public void run() {
            this.server.start();
        }

        @Override
        public InternetPacket invoke(InternetPacket packet) {
            StreamPacket streamPacket = new StreamPacket();
            streamPacket = streamPacket.toObject(packet.getData());
            if (streamPacket != null) {
                if (callbacks.containsKey(streamPacket.getGuid())) {
                    IStreamCallback callback = callbacks.get(streamPacket.getGuid());
                    Task task = new Task(callback, streamPacket);
                    this.threadPool.execute(task);
                }
            }
            return InternetPacket.getUnknowPacket();
        }
    }

    /*
     * All slots.
     */
    private final List<Slot> slots;

    /**
     * Thread pool.
     */
    private final ExecutorService threadPool;

    /**
     * Single instance.
     */
    private static Communicator instance;

    private Communicator() {
        this.slots = new ArrayList<>();
        this.threadPool = Executors.newFixedThreadPool(SystemConfig.slotCount);
        for (int i = 0; i < SystemConfig.slotCount; i++) {
            Slot slot = new Slot(i);
            this.slots.add(slot);
            ZookeeperClient.getInstance().setNodeValue(SystemConfig.slaveRoot + "/" + SystemConfig.crSlaveAddress + "," + SystemConfig.crSlavePort + "/slots/" + slot.getName(), slot.getConnectString());
            this.threadPool.execute(slot);
        }
    }

    /**
     * Get best performace slot.
     *
     * @return
     */
    public Slot selectSlot() {
        int index = 0;
        int min = Integer.MAX_VALUE;
        for (int i = 0; i < this.slots.size(); i++) {
            if (this.slots.get(i).getUsingCount() < min) {
                index = i;
                min = this.slots.get(i).getUsingCount();
            }
        }
        return this.slots.get(index);
    }

    /**
     * Get all slots.
     *
     * @return
     */
    public List<Slot> getAllSlots() {
        return this.slots;
    }

    /**
     * Get Single instance.
     *
     * @return
     */
    public static Communicator getInstance() {
        if (instance == null) {
            instance = new Communicator();
        }
        return instance;
    }

    /**
     * Get used slot count.
     *
     * @return
     */
    public int getUsedSlotCount() {
        int count = 0;
        count = slots.stream().map((slot) -> slot.getUsingCount()).reduce(count, Integer::sum);
        return count;
    }
}
