/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer;

import com.iveely.framework.net.websocket.EchoWebSocket;
import com.iveely.analyzer.service.SCallback;
import com.iveely.analyzer.service.WebSocketRequest;
import java.io.IOException;

/**
 *
 * @author X1 Carbon
 */
public class Program {

    /**
     * @param args the command line arguments
     */
    public static void main2(String[] args) {
        start();
    }

    private static void start() {
        try {
            SCallback callback = new SCallback();
            Thread syncThread = new Thread(callback);
            syncThread.start();

            WebSocketRequest request = new WebSocketRequest(callback);
            System.out.println("Web socket is starting.");
            EchoWebSocket socket = new EchoWebSocket(request, 9011);
            socket.service();
        } catch (IOException ex) {
            ex.printStackTrace();
        }
    }

}
