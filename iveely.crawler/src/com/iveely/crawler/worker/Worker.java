package com.iveely.crawler.worker;


import com.iveely.crawler.config.Loader;
import com.iveely.crawler.config.Seed;
import com.iveely.framework.process.ThreadUtil;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class Worker implements Runnable {

  private static final Logger logger =
      LoggerFactory.getLogger(Worker.class);

  private final WorkerHandler handler;

  private static Map<String, Seed> tasks;

  private static Object mutex = new Object();

  public Worker() {
    this.handler = new WorkerHandler();
    this.tasks = new HashMap<>();
  }

  public static void remove(final String name) {
    synchronized (mutex) {
      if (tasks.containsKey(name)) {
        tasks.remove(name);
      }
    }
  }

  @Override
  public void run() {
    while (true) {
      List<Seed> configs = Loader.fromLocal();
      synchronized (mutex) {
        for (Seed seed : configs) {
          if (!tasks.containsKey(seed.getName())) {
            tasks.put(seed.getName(), seed);
            this.handler.push(seed);
          }
        }
      }
      ThreadUtil.sleep(60 * 5);
    }
  }
}
