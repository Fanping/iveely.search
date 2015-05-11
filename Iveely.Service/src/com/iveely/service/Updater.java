/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.service;

/**
 *
 * @author 凡平
 */
public class Updater implements Runnable {

    @Override
    public void run() {
        while (true) {
            try {
                Config.nofity();
                Config.update();
                Thread.sleep(1000 * 60 * 60 * 6);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

}
