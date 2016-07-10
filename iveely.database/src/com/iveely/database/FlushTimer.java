/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.database;

/**
 * @author Administrator
 */
public class FlushTimer implements Runnable {

  @Override
  public void run() {
    while (true) {
      try {
        String[] names = LocalStore.getDatabases();
        for (String name : names) {
          LocalStore.getWarehouse(name).close();
        }
        Thread.sleep(1000 * 60 * 10);
      } catch (Exception ex) {
        ex.printStackTrace();
      }
    }
  }
}
