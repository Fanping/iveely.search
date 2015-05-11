package com.iveely.framework.net.websocket;

import org.apache.log4j.Logger;
import org.eclipse.jetty.websocket.api.RemoteEndpoint;
import org.eclipse.jetty.websocket.api.Session;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketClose;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketConnect;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketError;
import org.eclipse.jetty.websocket.api.annotations.OnWebSocketMessage;
import org.eclipse.jetty.websocket.api.annotations.WebSocket;
import org.eclipse.jetty.websocket.server.WebSocketHandler;
import org.eclipse.jetty.websocket.servlet.WebSocketServletFactory;

/**
 *
 * @author liufanping@iveely.com
 * @date 2014-12-7 20:32:30
 */
@WebSocket
public class WSHandler extends WebSocketHandler {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(WSHandler.class.getName());

    /**
     * Current connector.
     */
    private RemoteEndpoint connector;

    /**
     * Event processor.
     */
    private static IEventProcessor eventProcessor;
    
    public static void setProcessor(IEventProcessor processor) {
        eventProcessor = processor;
    }
    
    @OnWebSocketClose
    public void onClose(int statusCode, String reason) {
    }
    
    @OnWebSocketError
    public void onError(Throwable t) {
    }
    
    @OnWebSocketConnect
    public void onConnect(Session session) {
        connector = session.getRemote();
    }
    
    @OnWebSocketMessage
    public void onMessage(String message) {
        System.out.println(message);
        try {
            if (eventProcessor != null) {
                connector.sendString(eventProcessor.invoke(message));
            } else {
                connector.sendString("Not set processor.");
            }
        } catch (Exception e) {
            logger.error(e);
        }
        
    }
    
    @Override
    public void configure(WebSocketServletFactory factory) {
        // TODO Auto-generated method stub
        factory.register(WSHandler.class);
    }
}
