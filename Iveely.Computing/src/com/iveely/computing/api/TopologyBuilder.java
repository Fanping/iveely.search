package com.iveely.computing.api;

import com.iveely.computing.status.SystemConfig;
import com.iveely.computing.zookeeper.ZookeeperClient;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * Topology Builder.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 20:03:59
 */
public class TopologyBuilder {

    /**
     * Local(false) or cluster.
     */
    public boolean isLocaMode;

    /**
     * Total count of worker.
     */
    private int workerCount;

    /**
     * Total count of slave.
     */
    private int slaveCount;

    /**
     * User config.
     */
    private final HashMap<String, Object> userConfig;

    /**
     * Output & Input counter.
     */
    private final HashMap<String, Integer> counter;

    /**
     * Name of the topology.
     */
    private final String name;

    /**
     * All input & output.
     */
    private final List<String> allputs;

    public TopologyBuilder(String name) {
        this.userConfig = new HashMap<>();
        this.workerCount = 0;
        this.counter = new HashMap<>();
        this.name = name;
        this.allputs = new ArrayList<>();
    }

    /**
     * Get topology name.
     *
     * @return
     */
    public String getName() {
        return this.name;
    }

    /**
     * Set count of slave if need.
     *
     * @param count
     */
    public void setSlave(int count) {
        this.slaveCount = count;
    }

    /**
     * Return count of slave.
     *
     * @return
     */
    public int getSlaveCount() {
        int allSlaveCount = ZookeeperClient.getInstance().getChildren(SystemConfig.slaveRoot).size();
        if (this.slaveCount < 1 || this.slaveCount > allSlaveCount) {
            this.slaveCount = allSlaveCount / 2 + 1;
        }
        return this.slaveCount;
    }

    /**
     * Get config of user.
     *
     * @return
     */
    public HashMap<String, Object> getUserConfig() {
        return userConfig;
    }

    /**
     * Set input workers.
     *
     * @param input
     * @param workerCount
     */
    public void setInput(IInput input, int workerCount) {
        List<IInput> list = new ArrayList<>();
        for (int i = 0; i < workerCount; i++) {
            IInput inputClone = input.cloneSelf();
            list.add(inputClone);
            this.allputs.add(inputClone.getClass().getName());
        }
        this.workerCount += workerCount;
        if (SystemConfig.inputs.containsKey(this.name)) {
            List<IInput> temp = SystemConfig.inputs.get(this.name);
            temp.addAll(list);
            SystemConfig.inputs.put(this.name, temp);
        } else {
            SystemConfig.inputs.put(this.name, list);
        }
    }

    /**
     * Set output workers.
     *
     * @param output
     * @param workerCount
     */
    public void setOutput(IOutput output, int workerCount) {
        List<IOutput> list = new ArrayList<>();
        for (int i = 0; i < workerCount; i++) {
            IOutput outputClone = output.cloneSelf();
            list.add(outputClone);
            this.allputs.add(output.getClass().getName());
        }
        this.workerCount += workerCount;
        if (SystemConfig.outputs.containsKey(this.name)) {
            List<IOutput> temp = SystemConfig.outputs.get(this.name);
            temp.addAll(list);
            SystemConfig.outputs.put(this.name, temp);
        } else {
            SystemConfig.outputs.put(this.name, list);
        }
    }

    /**
     * Config information.
     *
     * @param key
     * @param val
     */
    public void put(String key, Object val) {
        userConfig.put(key, val);
    }

    /**
     * Get total count of workers.
     *
     * @return
     */
    public int getTotalWorkerCount() {
        return this.workerCount;
    }

    /**
     * Get all puts (input & output)
     *
     * @return
     */
    public List<String> getAllputs() {
        return this.allputs;
    }
}
