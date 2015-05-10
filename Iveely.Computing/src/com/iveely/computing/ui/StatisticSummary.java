package com.iveely.computing.ui;

import com.iveely.computing.zookeeper.ZookeeperClient;
import com.iveely.framework.text.json.JsonUtil;
import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-8 17:54:09
 */
public class StatisticSummary {

    /**
     * Response type.
     */
    private String resType;

    public String getResType() {
        return this.resType;
    }

    private String zBuffer;

    public String getZBuffer() {
        return this.zBuffer;
    }

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

        public String toJson() {
            return JsonUtil.beanToJson(this);
        }
    }

    public void init(String tpName) {

        // 1. type
        this.resType = "statistic summary";

        // 2. data
        this.zBuffer = "[";
        List<String> taskType = new ArrayList<>();
        taskType.add("input");
        taskType.add("output");
        taskType.stream().forEach((String tt) -> {
            List<String> inputOrOutputs = new ArrayList<>();
            List<String> allClasses = ZookeeperClient.getInstance().getChildren("/app/" + tpName + "/" + tt);
            allClasses.stream().map((clas) -> ZookeeperClient.getInstance().getChildren("/app/" + tpName + "/" + tt + "/" + clas)).filter((temp) -> (temp.size() > 0)).forEach((temp) -> {
                temp.stream().forEach((t) -> {
                    inputOrOutputs.add(t);
                });
            });

            inputOrOutputs.stream().map((ioput) -> {
                StatDetail detail = new StatDetail();
                detail.setGuid(ioput);
                // 2.1 emit
                String val = ZookeeperClient.getInstance().getNodeValue("/app/" + tpName + "/statistic/" + tt + "/" + ioput + "/emit");
                if (val.isEmpty()) {
                    detail.setEmitCount("0");
                } else {
                    detail.setEmitCount(val);
                }
                // 2.2 emit avg
                val = ZookeeperClient.getInstance().getNodeValue("/app/" + tpName + "/statistic/" + tt + "/" + ioput + "/emit_avg");
                if (val.isEmpty()) {
                    detail.setEmitAvg("0");
                } else {
                    detail.setEmitAvg(val);
                }
                // 2.3 execute
                val = ZookeeperClient.getInstance().getNodeValue("/app/" + tpName + "/statistic/" + tt + "/" + ioput + "/execute");
                if (val.isEmpty()) {
                    detail.setExecuteCount("0");
                } else {
                    detail.setExecuteCount(val);
                }
                // 2.4 execute avg
                val = ZookeeperClient.getInstance().getNodeValue("/app/" + tpName + "/statistic/" + tt + "/" + ioput + "/execute_avg");
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

    public String toJson() {
        return JsonUtil.beanToJson(this);
    }
}
