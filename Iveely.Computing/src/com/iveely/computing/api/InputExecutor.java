package com.iveely.computing.api;

import com.iveely.computing.node.Communicator;
import com.iveely.computing.zookeeper.ZookeeperClient;
import java.util.Date;
import java.util.HashMap;

/**
 * Input Executor.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 21:08:24
 */
public class InputExecutor extends IExecutor implements Runnable {

    /**
     * Input worker.
     */
    private final IInput _input;

    /**
     * Slot flag.
     */
    private final Communicator.Slot _slot;

    public InputExecutor(String name, IInput input, HashMap<String, Object> conf) {
        this._input = input;
        this._conf = conf;
        this._deDeclarer = new FieldsDeclarer();
        this._name = name;
        this._collector = new StreamChannel(_input.getName(), name, this);
        this._slot = Communicator.getInstance().selectSlot();
        ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/input/" + this._input.getClass().getName() + "/" + this._input.getName(), this._slot.getConnectString());
    }

    @Override
    public void run() {
        try {
            this._input.start(this._conf, this._collector);
            this._input.declareOutputFields(this._deDeclarer);
            this._input.toOutput();
            long start = new Date().getTime();
            while (!this._collector.hasEnd()) {
                this._input.nextTuple();
            }
            this._input.end(this._conf);
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
            ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/status/input/" + this._input.getName() + "/" + this._slot.getConnectString(), error.toString());
            ZookeeperClient.getInstance().setNodeValue("/app/" + this._name + "/finished", -1 + "");
        }

    }

    /**
     * Stop action.
     */
    public void stop() {
        this._collector.emitEnd();
    }
}
