package com.iveely.computing.ui;

import com.iveely.computing.zookeeper.ZookeeperClient;
import com.iveely.framework.text.json.JsonUtil;
import java.util.List;

/**
 * Topology summary.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-8 12:16:50
 */
public class TopologySummary {

    /**
     * Response type.
     */
    private String resType;

    /**
     * Get response type.
     *
     * @return
     */
    public String getResType() {
        return this.resType;
    }

    /**
     * All topology simple information.
     */
    private String zBuffer;

    public static class TopologySimple {

        public TopologySimple(String name) {
            this.name = name;
            this.id = name.hashCode();
        }

        /**
         * Name of the topology.
         */
        private final String name;

        public String getName() {
            return this.name;
        }

        /**
         * Id of the topology.
         */
        private final int id;

        public int getId() {
            return this.id;
        }

        /**
         * setupTime of the topology.
         */
        private String setupTime;

        public String getSetupTime() {
            return this.setupTime;
        }

        /**
         * Status of topology.
         */
        private String status;

        public String getStatus() {
            return this.status;
        }

        /**
         * How many slaves run this topology.
         */
        private int inSlaveCount;

        public int getInSlaveCount() {
            return this.inSlaveCount;
        }

        /**
         * How many thread run this topology.
         */
        private int threadCount;

        public int getThreadCount() {
            return this.threadCount;
        }

        public void init() {
            // 1. Status.
            int count = Integer.parseInt(ZookeeperClient.getInstance().getNodeValue("/app/" + this.name + "/finished"));
            if (count == 0) {
                this.status = "Completed";
            } else if (count > 1) {
                this.status = "Running";
            } else {
                this.status = "Exception";
            }

            // 2. Setuptime.
            String time = ZookeeperClient.getInstance().getNodeValue("/app/" + this.name);
            this.setupTime = time;

            // 3. InSlave
            this.inSlaveCount = Integer.parseInt(ZookeeperClient.getInstance().getNodeValue("/app/" + this.name + "/slavecount"));

            // 4. Thread count.
            this.threadCount = ZookeeperClient.getInstance().getChildren("/app/" + this.name).size();
        }

        public String toJson() {
            return JsonUtil.beanToJson(this);
        }
    }

    /**
     * Get buffer.
     *
     * @return
     */
    public String getZBuffer() {
        return this.zBuffer;
    }

    /**
     * Initialize.
     */
    public void init() {
        // 1. Response type.
        this.resType = "topology summary";

        // 2. Build topologys.
        List<String> names = ZookeeperClient.getInstance().getChildren("/app");
        this.zBuffer = "[";
        if (names.size() > 0) {
            names.stream().map((name) -> new TopologySimple(name)).map((simple) -> {
                simple.init();
                return simple;
            }).forEach((simple) -> {
                this.zBuffer += simple.toJson() + ",";
            });
        }
        this.zBuffer = this.zBuffer.substring(0, this.zBuffer.length() - 1);
        this.zBuffer += "]";
    }

    /**
     * Query to json.
     *
     * @param tpName
     * @return
     */
    public String queryToJson(String tpName) {
        // 1. Response type.
        this.resType = "query topology";

        // 2. Build topologys.
        List<String> names = ZookeeperClient.getInstance().getChildren("/app");
        this.zBuffer = "[";
        if (names.size() > 0) {
            names.stream().filter((name) -> (name.equals(tpName))).map((name) -> new TopologySimple(name)).map((simple) -> {
                simple.init();
                return simple;
            }).forEach((simple) -> {
                this.zBuffer += simple.toJson() + ",";
            });
        }
        this.zBuffer = this.zBuffer.substring(0, this.zBuffer.length() - 1);
        this.zBuffer += "]";
        return toJson();
    }

    /**
     * Topology summary to json.
     *
     * @return
     */
    public String toJson() {
        return JsonUtil.beanToJson(this);
    }
}
