/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.service;

import com.iveely.database.common.Configurator;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 *
 * @author X1 Carbon
 */
public class SCallback implements Runnable {

    private Cache cache;

    public SCallback() {
        init();
    }

    /**
     * Init.
     */
    private void init() {
        Object obj = Configurator.load("cache.c");
        if (obj == null) {
            cache = new Cache();
        } else {
            cache = (Cache) obj;
        }
    }

    public String query(String json) {
        return cache.query(json);
    }

    public String suggest(String json) {
        return cache.suggest(json);
    }

    public String recommend(String json) {
        return cache.recommend(json);
    }

    public String detail(String json) {
        return cache.detail(json);
    }

    public String image(String json) {
        return cache.image(json);
    }

    @Override
    public void run() {
        while (true) {
            try {
                cache.load();
                Thread.sleep(1000 * 60 * 60);
            } catch (InterruptedException ex) {
                Logger.getLogger(SCallback.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
    }
}
