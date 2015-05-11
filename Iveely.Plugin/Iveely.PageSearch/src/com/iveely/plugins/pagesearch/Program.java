/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.plugins.pagesearch;

import com.iveely.database.common.Configurator;
import com.iveely.framework.net.Server;
import com.iveely.plugins.pagesearch.data.Cache;

/**
 *
 * @author 凡平
 */
public class Program {

    /**
     * @param args the command line arguments
     */
    public static void main(String[] args) {
        com.iveely.framework.segment.DicSegment.getInstance();
        Cache cache = null;
        Object obj = Configurator.load("pagesearch.c");
        if (obj == null) {
            cache = new Cache();
        } else {
            cache = (Cache) obj;
        }
        Thread updater = new Thread(cache);
        updater.start();

        int port = 5005;
        System.out.println("Server started, port = " + port);
        EventHandler handler = new EventHandler(cache);
        Server server = new Server(handler, port);
        server.start();
    }

}
