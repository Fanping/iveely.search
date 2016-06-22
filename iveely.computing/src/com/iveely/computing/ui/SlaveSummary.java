/*
 * Copyright 2016 liufanping@iveely.com.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.iveely.computing.ui;

import com.iveely.computing.config.ConfigWrapper;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.computing.host.Luggage;
import com.iveely.framework.text.JSONUtil;

import java.util.List;

/**
 * Slave information summary.
 */
public class SlaveSummary {

    /**
     * Response type.
     */
    private String resType;

    /**
     * @return Get response type information,actually is "slave summary".
     */
    public String getResType() {
        return this.resType;
    }

    /**
     * All slave simple information.
     */
    private String zBuffer;

    /**
     * @return Get buffer, json data.
     */
    public String getZBuffer() {
        return this.zBuffer;
    }

    /**
     * Slave simple summary information.
     */
    public static class SlaveSimple {

        /**
         * Build slave simple instance.
         *
         * @param host The host of slave.
         */
        public SlaveSimple(String host) {
            this.host = host;
            this.id = host.hashCode();
        }

        /**
         * Id of the slave.
         */
        private final int id;

        /**
         * @return Get slave id.
         */
        public int getId() {
            return this.id;
        }

        /**
         * Host of the slave.
         */
        private final String host;

        /**
         * @return Get host address of the slave.
         */
        public String getHost() {
            return this.host;
        }

        /**
         * Setup time.
         */
        private String setupTime;

        /**
         * @return Get slave setup time.
         */
        public String getSetupTime() {
            return this.setupTime;
        }

        /**
         * Count of slots.
         */
        private int slotsCount;

        /**
         * @return Get total slot count.
         */
        public int getSlotsCount() {
            return this.slotsCount;
        }

        /**
         * How many app on this slave.
         */
        private int runningApp;

        /**
         *
         * @return Get applications count which in runing.
         */
        public int getRunningApp() {
            return this.runningApp;
        }

        /**
         * Initialize the basic information.
         */
        public void init() {
            // 1. Running app.
            this.runningApp = Luggage.slaves.get(this.host);

            // 2. slots count.
            this.slotsCount = ConfigWrapper.get().getSlave().getSlotCount();

            // 3. setup time.
            this.setupTime = Coordinator.getInstance().getNodeValue(ConfigWrapper.get().getSlave().getRoot() + "/" + this.host);
        }

        /**
         * @return Convert to JSON data.
         */
        public String toJson() {
            return JSONUtil.toString(this);
        }
    }

    /**
     * Get slave summary information.
     */
    public void get() {
        // 1. Response type.
        this.resType = "slave summary";

        // 2. Build topologys.
        List<String> names = Coordinator.getInstance().getChildren(ConfigWrapper.get().getSlave().getRoot());
        this.zBuffer = "[";
        if (names.size() > 0) {
            names.stream().map((name) -> new SlaveSimple(name)).map((simple) -> {
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
     * Current summary to json.
     *
     * @return
     */
    public String toJson() {
        return JSONUtil.toString(this);
    }
}
