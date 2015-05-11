/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.service;

import com.iveely.framework.net.websocket.EchoWebSocket;
import com.iveely.online.WebSocketRequest;
import java.io.IOException;

/**
 *
 * @author 凡平
 */
public class Program {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        // 1. Updater.
        Thread updater = new Thread(new Updater());
        updater.start();

        // 2. Service.
        try {
            WebSocketRequest request = new WebSocketRequest();
            System.out.println("Web socket is starting.");
            EchoWebSocket socket = new EchoWebSocket(request, 9011);
            socket.service();
        } catch (IOException ex) {
            ex.printStackTrace();
        }
    }
}
