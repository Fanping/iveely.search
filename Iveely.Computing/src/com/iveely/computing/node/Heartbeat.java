package com.iveely.computing.node;

import com.iveely.computing.common.Message;
import com.iveely.computing.config.Configurator;
import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.net.InetAddress;
import java.net.UnknownHostException;
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
    private Client client;

    /**
     * Service port.
     */
    private int port;

    /**
     * Service ipaddress.
     */
    private String ip;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Heartbeat.class.getName());

    public Heartbeat(int port) {
        try {
            this.port = port;
            this.ip = InetAddress.getLocalHost().getHostAddress();
            client = new Client(Configurator.getMasterAddress(), Configurator.getMasterPort());
        } catch (UnknownHostException ex) {
            logger.error(ex);
        }
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
                    try {
                        BufferedWriter out = new BufferedWriter(new FileWriter("Common/allClients.txt"));
                        out.write(allClients);
                        out.close();
                    } catch (IOException ex) {
                        logger.error(ex);
                    }
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
        return this.ip + ":" + this.port + ":" + Attribute.getInstance().getRunningAppsCount();
    }
}
