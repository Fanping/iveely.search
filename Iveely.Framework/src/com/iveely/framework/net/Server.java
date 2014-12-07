package com.iveely.framework.net;

import java.io.IOException;
import java.net.ServerSocket;
import java.net.Socket;
import org.apache.log4j.Logger;

/**
 * Net connector - Server.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 10:57:51
 */
public class Server {

    /**
     * Client count.
     */
    private final int currentClientCount;

    /**
     * Multithreading lock object.
     */
    private static Object threadObj;

    /**
     * Max client count.
     */
    private final int MAX_CLIENT_COUNT = 100;

    /**
     * Service port.
     */
    private final int port;

    /**
     * Call back of the message.
     */
    private ICallback callback;

    /**
     * Logger
     */
    private final Logger logger = Logger.getLogger(Server.class.getName());

    public Server(ICallback callback, int port) {
        currentClientCount = 0;
        threadObj = new Object();
        if (port > 0 && port < 65535) {
            this.port = port;
        } else {
            logger.error(port + " is not in 0~65535");
            this.port = -1;
        }
        if (callback != null) {
            this.callback = callback;
        } else {
            logger.error("Call back function can not be null.");
        }
    }

    /**
     * Start service.
     */
    public void start() {
        try {
            ServerSocket serverSocket = new ServerSocket(port);
            logger.info("Socket server started.");
            while (true) {
                Socket socket = serverSocket.accept();
                logger.info("get connection from:" + socket.getRemoteSocketAddress().toString());
                // 任务交付给执行者去做
                ServerExecutor executor = new ServerExecutor(socket, callback);
                executor.start();
            }
        } catch (IOException ex) {
            logger.error(ex);
        }
    }

    /**
     * Get the service of port.
     *
     * @return
     */
    public int getPort() {
        return this.port;
    }
}
