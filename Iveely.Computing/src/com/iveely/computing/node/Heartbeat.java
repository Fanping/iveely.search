package com.iveely.computing.node;

import com.iveely.computing.common.Message;
import com.iveely.computing.status.SystemConfig;
import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import org.apache.log4j.Logger;

/**
 * Heartbeat
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 15:20:18
 */
public class Heartbeat implements Runnable {

    /**
     * Message client.
     */
    private final Client client;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Heartbeat.class.getName());

    public Heartbeat() {
        SystemConfig.crSlaveAddress = com.iveely.framework.net.Internet.getLocalIpAddress();
        client = new Client(SystemConfig.masterServer, SystemConfig.masterPort);
    }

    @Override
    public void run() {
        while (true) {
            try {
                InternetPacket packet = new InternetPacket();
                packet.setExecutType(Message.ExecutType.HEARTBEAT.ordinal());
                packet.setMimeType(Message.MIMEType.MESSAGE.ordinal());
                packet.setData(Message.getBytes(beatInfo()));
                InternetPacket response = client.send(packet);
                if (response != null) {
                    String allClients = Message.getString(response.getData());
                    logger.info("All client:" + allClients);
                }
                Thread.sleep(1000 * 30);
            } catch (InterruptedException ex) {
                logger.error("Send heartbeat stoped:" + ex);
                return;
            }
        }
    }

    /**
     * Heartbeat information.
     *
     * @return
     */
    private String beatInfo() {
        return SystemConfig.crSlaveAddress + ":" + SystemConfig.crSlavePort + ":" + Communicator.getInstance().getUsedSlotCount();
    }
}
