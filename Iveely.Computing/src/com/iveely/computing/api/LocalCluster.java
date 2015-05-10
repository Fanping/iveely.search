package com.iveely.computing.api;

import com.iveely.computing.host.Master;
import com.iveely.computing.node.Slave;
import com.iveely.computing.status.SystemConfig;
import com.iveely.computing.zookeeper.ZookeeperClient;
import com.iveely.computing.zookeeper.ZookeeperServer;
import java.util.HashMap;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import org.apache.log4j.Logger;

/**
 * Local cluster running environment.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 21:35:22
 */
public class LocalCluster {

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(LocalCluster.class.getName());

    /**
     * Total count of workers.
     */
    private int workerCount;

    /**
     * Thread pool.
     */
    private ExecutorService threadPool;

    /**
     * User config.
     */
    private HashMap<String, Object> userConfig;

    /**
     * input config.
     */
    private List<IInput> inputConfig;

    /**
     * output config.
     */
    private List<IOutput> outputConfig;

    /**
     * Submit to local cluster.
     *
     * @param builder
     */
    public void submit(TopologyBuilder builder) {

        // 0. Init.
        this.workerCount = builder.getTotalWorkerCount();
        this.threadPool = Executors.newFixedThreadPool(3 + workerCount);

        // 1. Run local cluster.
        if (!runCluster()) {
            return;
        }

        // 2. Build config.
        ZookeeperClient.getInstance().deleteNode("/app/" + builder.getName());
        userConfig = builder.getUserConfig();
        inputConfig = SystemConfig.inputs.get(builder.getName());
        outputConfig = SystemConfig.outputs.get(builder.getName());

        // 3. Try to run output.
        for (int i = outputConfig.size() - 1; i > -1; i--) {
            IOutput output = outputConfig.get(i);
            String name = output.getName();
            logger.info(output.getName());
            OutputExecutor executor = new OutputExecutor(builder.getName(), output, userConfig);
            threadPool.execute(executor);
        }

        // 4. Try to run input.
        for (int i = inputConfig.size() - 1; i > -1; i--) {
            IInput input = inputConfig.get(i);
            logger.info("run " + input.getName());
            InputExecutor executor = new InputExecutor(builder.getName(), input, userConfig);
            threadPool.execute(executor);
        }
    }

    /**
     * Run local cluster.
     */
    private boolean runCluster() {
        try {
            com.iveely.framework.segment.DicSegment.getInstance();

            ZookeeperServer zookeeper = new ZookeeperServer();
            logger.info("starting zookeeper...");
            threadPool.execute(zookeeper);
            Thread.sleep(2000);

            // 1. Run master.
            logger.info("starting master...");
            Master master = new Master("127.0.0.1", 2181);
            threadPool.execute(master);

            // 2. Run slave.
            logger.info("starting slave on 7001...");
            Slave slave = new Slave("127.0.0.1", 2181);
            threadPool.execute(slave);
            return true;
        } catch (Exception e) {
            logger.error(e);
        }
        return false;
    }

}
