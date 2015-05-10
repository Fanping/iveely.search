package com.iveely.computing.zookeeper;

import org.apache.zookeeper.server.quorum.QuorumPeerMain;

/**
 * Server of zookeeper.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-5 16:30:08
 */
public class ZookeeperServer implements Runnable {

    @Override
    public void run() {
        QuorumPeerMain.main(new String[]{"zoo.cfg"});
    }
}
