package com.iveely.framework.net.websocket;

import java.io.IOException;
import java.net.ServerSocket;
import org.apache.log4j.Logger;
import org.eclipse.jetty.server.Server;

/**
 * Websocket.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-2 22:26:28
 */
public class EchoWebSocket {

    /**
     * Service port.
     */
    private int port = 8000;

    /**
     * Service socket.
     */
    private ServerSocket serverSocket;

    /**
     * Event processor.
     */
    private IEventProcessor eventProcessor;

    /**
     * logger.
     */
    private final Logger logger = Logger.getLogger(EchoWebSocket.class.getName());

    public EchoWebSocket(IEventProcessor processor, int port) throws IOException {
        if (processor == null) {
            throw new NullPointerException();
        }
        this.port = port;
        this.eventProcessor = processor;
        logger.info("Web socket service is init.");
    }

    public void service() {
        try {
            Server server = new Server(this.port);
            WSHandler.setProcessor(eventProcessor);
            server.setHandler(new WSHandler());
            server.setStopTimeout(0);
            server.start();
            server.join();
        } catch (Exception ex) {
            logger.error(ex);
        }
    }
}
