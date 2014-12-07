package com.iveely.framework.net;

import java.io.IOException;
import java.net.InetAddress;
import java.net.ServerSocket;
import java.net.UnknownHostException;

/**
 * Internet
 *
 * @author liufanping@iveely.com
 * @date 2014-11-13 20:24:13
 */
public class Internet {

    /**
     * Get avaliable port on local machine.
     *
     * @param start
     * @return
     */
    public static int getAvaliablePort(int start) {
        for (int i = start; i < 65536; i++) {
            try {
                ServerSocket socket = new ServerSocket(i);
                if (socket != null) {
                    socket.close();
                }
                return i;
            } catch (IOException exception) {

            }
        }
        return -1;
    }

    /**
     * Get ip address of local machine.
     *
     * @return
     */
    public static String getLocalIpAddress() {
        try {
            InetAddress addr = InetAddress.getLocalHost();
            String ip = addr.getHostAddress();
            return ip;
        } catch (UnknownHostException ex) {
            return null;
        }
    }
}
