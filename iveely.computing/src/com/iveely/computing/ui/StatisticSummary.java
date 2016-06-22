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

import com.iveely.computing.config.Paths;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.framework.text.JSONUtil;

import java.util.ArrayList;
import java.util.List;

/**
 * Statistic summary inforamton,For a specify topology statistics.
 *
 */
public class StatisticSummary {

    /**
     * Response type.
     */
    private String resType;

    /**
     * @return Get resonpse type,actually is "statistic summary'.
     */
    public String getResType() {
        return this.resType;
    }

    /**
     * JSON data.
     */
    private String zBuffer;

    /**
     * @return Get content by JSON data.
     */
    public String getZBuffer() {
        return this.zBuffer;
    }

    /**
     * Statistic detail information.
     */
    public static class StatDetail {

        /**
         * Guid of the inputOrOutput.
         */
        private String guid;

        /**
         * @return the guid
         */
        public String getGuid() {
            return guid;
        }

        /**
         * Set guid by id.
         *
         * @param id The id to set.
         */
        public void setGuid(String id) {
            this.guid = id;
        }

        /**
         * Total emit count.
         */
        private String emitCount;

        /**
         * Avage of emit.
         */
        private String emitAvg;

        /**
         * Total execute count.
         */
        private String executeCount;

        /**
         * Avage of execute.
         */
        private String executeAvg;

        /**
         * @return the emitCount
         */
        public String getEmitCount() {
            return emitCount;
        }

        /**
         * @param emitCount the emitCount to set
         */
        public void setEmitCount(String emitCount) {
            this.emitCount = emitCount;
        }

        /**
         * @return the emitAvg
         */
        public String getEmitAvg() {
            return emitAvg;
        }

        /**
         * @param emitAvg the emitAvg to set
         */
        public void setEmitAvg(String emitAvg) {
            this.emitAvg = emitAvg;
        }

        /**
         * @return the executeCount
         */
        public String getExecuteCount() {
            return executeCount;
        }

        /**
         * @param executeCount the executeCount to set
         */
        public void setExecuteCount(String executeCount) {
            this.executeCount = executeCount;
        }

        /**
         * @return the executeAvg
         */
        public String getExecuteAvg() {
            return executeAvg;
        }

        /**
         * @param executeAvg the executeAvg to set
         */
        public void setExecuteAvg(String executeAvg) {
            this.executeAvg = executeAvg;
        }

        /**
         *
         * @return JSON data of serilize this.
         */
        public String toJson() {
            return JSONUtil.toString(this);
        }
    }

    /**
     * Get statistic summary for a specify topology.
     *
     * @param tpName Name of the toplogy.
     */
    public void get(String tpName) {

        // 1. type
        this.resType = "statistic summary";

        // 2. data
        this.zBuffer = "[";
        List<String> taskType = new ArrayList<>();
        taskType.add("input");
        taskType.add("output");
        taskType.stream().forEach((String tt) -> {
            List<String> inputOrOutputs = new ArrayList<>();
            List<String> allClasses = Coordinator.getInstance().getChildren(Paths.getTopologyRoot(tpName) + "/" + tt);
            allClasses.stream()
                    .map((clas) -> Coordinator.getInstance().getChildren(Paths.getTopologyRoot(tpName) + "/" + tt + "/" + clas))
                    .filter((temp) -> (temp.size() > 0)).forEach((temp) -> {
                        temp.stream().forEach((t) -> {
                            inputOrOutputs.add(t);
                        });
                    });

            inputOrOutputs.stream().map((ioput) -> {
                StatDetail detail = new StatDetail();
                detail.setGuid(ioput);
                // 2.1 emit
                String val = Coordinator.getInstance()
                        .getNodeValue(Paths.getInputTotalEmit(tpName, ioput));
                if (val.isEmpty()) {
                    detail.setEmitCount("0");
                } else {
                    detail.setEmitCount(val);
                }
                // 2.2 emit avg
                val = Coordinator.getInstance()
                        .getNodeValue(Paths.getInputAvgEmit(tpName, ioput));
                if (val.isEmpty()) {
                    detail.setEmitAvg("0");
                } else {
                    detail.setEmitAvg(val);
                }
                // 2.3 execute
                val = Coordinator.getInstance()
                        .getNodeValue(Paths.getTopologyOutputExecute(tpName, ioput));
                if (val.isEmpty()) {
                    detail.setExecuteCount("0");
                } else {
                    detail.setExecuteCount(val);
                }
                // 2.4 execute avg
                val = Coordinator.getInstance()
                        .getNodeValue(Paths.getTopologyOutputAvgExecute(tpName, ioput));
                if (val.isEmpty()) {
                    detail.setExecuteAvg("0");
                } else {
                    detail.setExecuteAvg(val);
                }
                return detail;
            }).forEach((detail) -> {
                this.zBuffer += detail.toJson() + ",";
            });
        });
        this.zBuffer = this.zBuffer.substring(0, this.zBuffer.length() - 1);
        this.zBuffer += "]";
    }

    /**
     * @return JSON data.
     */
    public String toJson() {
        return JSONUtil.toString(this);
    }
}
