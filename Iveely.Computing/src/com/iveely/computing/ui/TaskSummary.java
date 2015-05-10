package com.iveely.computing.ui;

import com.iveely.computing.zookeeper.ZookeeperClient;
import com.iveely.framework.text.json.JsonUtil;
import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-8 16:21:24
 */
public class TaskSummary {

    /**
     * Response type.
     */
    private String resType;

    public String getResType() {
        return this.resType;
    }

    /**
     * Buffer of child.
     */
    private String zBuffer;

    /**
     * Get buffer.
     *
     * @return
     */
    public String getZBuffer() {
        return this.zBuffer;
    }

    public static class TaskDetail {

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
         * Connect net path.
         */
        private String connectPath;

        /**
         * @return the connectPath
         */
        public String getConnectPath() {
            return connectPath;
        }

        /**
         * Exception on this task.
         */
        private String exception;

        /**
         * @return the exception
         */
        public String getException() {
            return exception;
        }

        private String type;

        public String toJson() {
            return JsonUtil.beanToJson(this);
        }

        /**
         * @return the type
         */
        public String getType() {
            return type;
        }

        /**
         * @param type the type to set
         */
        public void setType(String type) {
            this.type = type;
        }

        /**
         * @param exception the exception to set
         */
        public void setException(String exception) {
            this.exception = exception;
        }

        /**
         * @param connectPath the connectPath to set
         */
        public void setConnectPath(String connectPath) {
            this.connectPath = connectPath;
        }

        /**
         * @param guid the guid to set
         */
        public void setGuid(String guid) {
            this.guid = guid;
        }
    }

    public void init(String tpName) {
        // 1. type.
        this.resType = "task summary";

        // 2. Get all task (Input & Output)
        this.zBuffer = "[";
        List<String> taskType = new ArrayList<>();
        taskType.add("input");
        taskType.add("output");
        taskType.stream().forEach((tt) -> {
            List<String> inputOrOutputs = new ArrayList<>();
            List<String> allClasses = ZookeeperClient.getInstance().getChildren("/app/" + tpName + "/" + tt);
            allClasses.stream().forEach((clas) -> {
                List<String> temp = ZookeeperClient.getInstance().getChildren("/app/" + tpName + "/" + tt + "/" + clas);
                if (temp.size() > 0) {
                    temp.stream().forEach((t) -> {
                        inputOrOutputs.add(clas + "/" + t);
                    });
                }
            });
            inputOrOutputs.stream().map((inputOrOutput) -> {
                String[] simDetail = inputOrOutput.split("/");
                TaskDetail task = new TaskDetail();
                task.setGuid(simDetail[1]);
                task.setType(tt);
                String connect = ZookeeperClient.getInstance().getNodeValue("/app/" + tpName + "/" + tt + "/" + inputOrOutput);
                task.setConnectPath(connect);
                List<String> exceptionSlots = ZookeeperClient.getInstance().getChildren("/app/" + tpName + "/status/" + tt + "/" + simDetail[1]);
                if (exceptionSlots.isEmpty()) {
                    task.setException(" ");
                } else {
                    String excpt = ZookeeperClient.getInstance().getNodeValue("/app/" + tpName + "/status/" + tt + "/" + simDetail[1] + "/" + exceptionSlots.get(0));
                    task.setException(excpt);
                }
                return task;
            }).forEach((task) -> {
                this.zBuffer += task.toJson() + ",";
            });
        });
        this.zBuffer = this.zBuffer.substring(0, this.zBuffer.length() - 1);
        this.zBuffer += "]";
    }

    /**
     * Task summary to json.
     *
     * @return
     */
    public String toJson() {
        return JsonUtil.beanToJson(this);
    }
}
