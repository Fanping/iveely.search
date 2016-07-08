package com.iveely.crawler.worker;

import com.iveely.crawler.config.Seed;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.concurrent.Executors;
import java.util.concurrent.ThreadPoolExecutor;

/**
 * @author Fanping liu (liufanping@iveely.com)
 */
public class WorkerHandler {

  private static Logger logger = LoggerFactory.getLogger(WorkerHandler.class);

  private static Integer THREAD_POOL_SIZE = 50;

  private final ThreadPoolExecutor thredpool;

  public WorkerHandler() {
    this.thredpool = (ThreadPoolExecutor) Executors.newFixedThreadPool(
        THREAD_POOL_SIZE);
  }

  public int getAvailableThreadCount() {
    return THREAD_POOL_SIZE - this.thredpool.getQueue().size() -
        this.thredpool.getActiveCount();
  }

  public void push(final Seed seed) {
    SeedExecutor executor = new SeedExecutor(seed, new SimpleParser());
    this.thredpool.execute(executor);
  }
}
