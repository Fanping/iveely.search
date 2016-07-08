package com.iveely.crawler.master;

import com.iveely.crawler.common.ExecuteType;
import com.iveely.crawler.config.Seed;
import com.iveely.crawler.config.Task;
import com.iveely.crawler.worker.BloomFilter;
import com.iveely.framework.net.AsynServer;
import com.iveely.framework.net.Packet;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * @author Fanping liu (liufanping@iveely.com)
 */
public class MasterHandler implements AsynServer.IHandler {

  private static Logger logger = LoggerFactory.getLogger(MasterHandler.class);

  private static Object mutex = new Object();

  private Map<String, Task> tasks;

  private BloomFilter<String> filter;

  public MasterHandler() {
    this.tasks = new HashMap<>();
  }

  public void addTasks(final List<Seed> configs) {
    synchronized (mutex) {
      for (Seed seed : configs) {
        if (tasks.containsKey(seed.getName())) {
          tasks.get(seed.getName()).setSeed(seed);
        } else {
          tasks.put(seed.getName(), new Task(seed));
        }
      }
    }
  }

  @Override
  public Packet process(Packet packet) {
    synchronized (mutex) {
      if (packet.getMimeType() == ExecuteType.HEARTBEAT.ordinal()) {
        Packet respPacket = new Packet();
        List<Task> seedList = getAvailableTask();
        int totalSize = seedList.size();
        int needCount = (int) packet.getData();
        if (totalSize > 0 && needCount > 0) {
          ArrayList<Seed> configs = new ArrayList<>();
          int max = needCount > totalSize ? totalSize : needCount;
          for (int i = 0; i < max; i++) {
            configs.add(seedList.get(i).getSeed());
            tasks.get(seedList.get(i).getSeed().getName())
                .setStatus(Task.Status.RUNNING);
          }
          respPacket.setExecuteType(ExecuteType.TASK_LIST.ordinal());
          respPacket.setData(configs);
        } else {
          respPacket.setExecuteType(ExecuteType.EMPTY.ordinal());
          respPacket.setData("");
        }
        return respPacket;
      } else {
        return Packet.getUnknownPacket();
      }
    }
  }

  public List<Task> getAvailableTask() {
    // 1. Get pending.
    List<Task> list = new ArrayList<>();
    for (Map.Entry<String, Task> entry : this.tasks.entrySet()) {
      Task task = entry.getValue();
      if (task.getStatus() == Task.Status.PENDING
          && (System.currentTimeMillis() - task.getEndtime()) > task.getSeed()
          .getFrequency() * 1000 * 60) {
        list.add(task);
      } else if (task.getStatus() == Task.Status.RUNNING && (System
          .currentTimeMillis() - task.getEndtime()) > 1000 * 60 * 60 * 6) {
        list.add(task);
      }
    }
    return list;
  }

  @Override
  public void caught(String exception) {
    logger.warn(exception);
  }
}

