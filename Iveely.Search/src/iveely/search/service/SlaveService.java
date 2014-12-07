package iveely.search.service;

import com.iveely.framework.net.Internet;
import com.iveely.framework.net.Server;
import com.iveely.framework.net.cache.Memory;
import iveely.search.store.UserLogger;
import java.net.InetAddress;
import java.net.UnknownHostException;

/**
 * Slave service.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-1 18:13:14
 */
public class SlaveService {

    /**
     * Server.
     */
    private Server server;

    /**
     * Flag of server.
     */
    private static String flag;

    /**
     * Message callback.
     */
    private SCallback callback;

    /**
     * Initialization.
     */
    private void init() {
        callback = new SCallback();
        callback.init();
        Thread daemon = new Thread(callback);
        daemon.start();
        UserLogger.init("UserLogger");
        UserLogger.getInstance();
    }

    /**
     *
     * Get Service Identity.
     *
     * @return
     */
    public static String getFlag() {
        return flag;
    }

    public String invoke(String arg) {
        init();

        //1. Prepare service port.
        int port = Internet.getAvaliablePort(6500);
        try {
            flag = InetAddress.getLocalHost().getHostAddress() + ":" + port;
            Memory.getInstance().initCache("Common/allClients.txt", false);
            Memory.getInstance().append("[SLAVESERVICE_TEXT]", flag);
            Memory.getInstance().append("[SLAVESERVICE_IMAGE]", flag);
        } catch (UnknownHostException e) {
        }
        server = new Server(callback, port);
        server.start();
        return "OK";
    }
}
