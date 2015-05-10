package com.iveely.computing.zookeeper;

import com.iveely.computing.common.Message;
import com.iveely.computing.status.SystemConfig;
import java.io.IOException;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.concurrent.CountDownLatch;
import org.apache.log4j.Logger;
import org.apache.zookeeper.CreateMode;
import org.apache.zookeeper.KeeperException;
import org.apache.zookeeper.WatchedEvent;
import org.apache.zookeeper.Watcher;
import org.apache.zookeeper.Watcher.Event.KeeperState;
import org.apache.zookeeper.ZooDefs;
import org.apache.zookeeper.ZooKeeper;
import org.apache.zookeeper.ZooKeeper.States;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-5 23:29:53
 */
public class ZookeeperClient {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(ZookeeperClient.class.getName());

    private ZookeeperClient() throws IOException {
        CountDownLatch connectedLatch = new CountDownLatch(1);
        Watcher watcher = new ConnectedWatcher(connectedLatch);
        zk = new ZooKeeper(SystemConfig.zkServer + ":" + SystemConfig.zkPort,
                10000, watcher);
        waitUntilConnected(zk, connectedLatch);
    }

    private static ZookeeperClient client;

    private final ZooKeeper zk;

    public static void waitUntilConnected(ZooKeeper zooKeeper, CountDownLatch connectedLatch) {
        if (States.CONNECTING == zooKeeper.getState()) {
            try {
                connectedLatch.await();
            } catch (InterruptedException e) {
                throw new IllegalStateException(e);
            }
        }
    }

    static class ConnectedWatcher implements Watcher {

        private final CountDownLatch connectedLatch;

        ConnectedWatcher(CountDownLatch connectedLatch) {
            this.connectedLatch = connectedLatch;
        }

        @Override
        public void process(WatchedEvent event) {
            if (event.getState() == KeeperState.SyncConnected) {
                connectedLatch.countDown();
            }
        }
    }

    public static ZookeeperClient getInstance() {
        if (client == null) {
            try {
                client = new ZookeeperClient();
            } catch (Exception e) {
                logger.error(e);
            }
        }
        return client;
    }

    /**
     * Get value of a path.
     *
     * @param path
     * @return
     */
    public String getNodeValue(String path) {
        checkNodeReady(path);
        try {
            byte[] data = zk.getData(path, false, null);
            return Message.getString(data);
        } catch (KeeperException | InterruptedException e) {
            logger.error(e);
        }
        return "";
    }

    /**
     * Set value of a path.
     *
     * @param path
     * @param val
     */
    public void setNodeValue(String path, String val) {
        try {
            checkNodeReady(path);
            if (zk.exists(path, false) != null) {
                zk.setData(path, Message.getBytes(val), -1);
            } else {
                zk.create(path, Message.getBytes(val),
                        ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
            }
        } catch (KeeperException | InterruptedException e) {
            logger.error(e);
        }
    }

    /**
     * Delete a path.
     *
     * @param path
     */
    public void deleteNode(String path) {
        List<String> list = getChildren(path);
        list.stream().map((list1) -> (path + "/" + list1).replace("//", "/")).forEach((childPath) -> {
            deleteNode(childPath);
        });
        try {
            if (zk.exists(path, false) != null) {
                zk.delete(path, -1);
            }
        } catch (KeeperException | InterruptedException ex) {
            logger.error(ex);
        }
    }

    /**
     * Get child nodes of a path.
     *
     * @param path
     * @return
     */
    public List<String> getChildren(String path) {
        List<String> children = new ArrayList<>();
        path = path.replace("//", "/");
        checkNodeReady(path);
        try {
            if (zk.exists(path, null) != null) {
                children = zk.getChildren(path, false);
            }
        } catch (KeeperException | InterruptedException e) {
            logger.error(e);
        }
        return children;
    }

    /**
     * Check node is ready.
     *
     * @param path
     */
    private void checkNodeReady(String path) {
        try {
            String[] paths = path.split("/");
            String tempPath = "";
            for (int i = 0; i < paths.length - 1; i++) {
                tempPath = tempPath + "/" + paths[i];
                tempPath = tempPath.replaceAll("//", "/");
                if (zk.exists(tempPath, null) == null) {
                    zk.create(tempPath, new Date().toString().getBytes(Charset.defaultCharset()), ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }

            }
        } catch (KeeperException | InterruptedException e) {
            logger.error(e);
        }
    }

}
