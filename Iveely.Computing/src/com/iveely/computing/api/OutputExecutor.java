package com.iveely.computing.api;

import com.iveely.computing.node.Communicator;
import com.iveely.computing.zookeeper.ZookeeperClient;
import java.util.Date;
import java.util.HashMap;
import org.apache.log4j.Logger;

/**
 * Output Executor.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 22:59:40
 */
public class OutputExecutor extends IExecutor implements Runnable {

    /**
     * Input worker.
     */
    private final IOutput _output;

    /**
     * Execute count for statistic.
     */
    private int _executeCount;

    /**
     * Start time.
     */
    private final long _startTime;

    /**
     * Slot.
     */
    private final Communicator.Slot _slot;

    /**
     * Logger
     */
    private final Logger logger = Logger.getLogger(OutputExecutor.class.getName());

    public OutputExecutor(String name, IOutput output, HashMap<String, Object> conf) {
        this._output = output;
        this._conf = conf;
        this._executeCount = 0;
        this._deDeclarer = new FieldsDeclarer();
        this._name = name;
        this._slot = Communicator.getInstance().selectSlot();
        this._collector = new StreamChannel(output.getName(), name, this);
        this._slot.register(_output.getName(), this._collector);
        this._startTime = new Date().getTime();
        String connectPath = this._slot.getConnectString();
        logger.info("Output Exexutor started:" + connectPath);
        ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/output/" + output.getClass().getName() + "/" + output.getName(), connectPath);
    }

    @Override
    public void run() {
        this._output.start(this._conf, this._collector);
        this._output.declareOutputFields(this._deDeclarer);
        this._output.toOutput();
    }

    /**
     * Invoke tuple when data arrive.
     *
     * @param tuple
     */
    public void invoke(Tuple tuple) {
        try {
            this._output.execute(tuple);
            this._executeCount++;
            if (this._executeCount % 100 == 0) {
                double hour = (new Date().getTime() - this._startTime) / (1000.0 * 60 * 60);
                ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/statistic/output/" + this._output.getName() + "/execute", this._executeCount + "");
                ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/statistic/output/" + this._output.getName() + "/execute_avg", this._executeCount / hour + "(execute)/h");
            }
        } catch (Exception e) {
            StringBuilder error = new StringBuilder();
            error.append(e.getMessage());
            error.append("<br/>");
            StackTraceElement[] trace = e.getStackTrace();
            for (StackTraceElement s : trace) {
                error.append("   ");
                error.append(s);
                error.append("<br/>");
            }
            ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/status/output/" + this._output.getName() + "/" + this._slot.getConnectString(), error.toString());
            ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/finished", -1 + "");
        }
    }

    /**
     * All data has finish send.
     */
    public void end() {
        this._slot.unregister(this._output.getName());
        this._output.end(this._conf);
        ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/statistic/output/" + this._output.getName() + "/execute", this._executeCount + "");
        String str = ZookeeperClient.getInstance().getNodeValue("/app/" + this._name + "/finished");
        int count = 1;
        if (!str.isEmpty()) {
            count = Integer.parseInt(str) - 1;
        }
        ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/finished", count + "");
    }
}
