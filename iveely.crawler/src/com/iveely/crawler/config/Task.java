package com.iveely.crawler.config;

/**
 * @author liufanping (liufanping@iveely.com)
 */
public class Task {

  public Task(final Seed seed) {
    this.status = Status.PENDING;
    this.seed = seed;
    this.startTime = 0L;
    this.endtime = 0L;
  }

  public enum Status {
    RUNNING,
    PENDING
  }

  private Seed seed;

  private Long startTime;

  private Long endtime;

  private Status status;

  public Status getStatus() {
    return status;
  }

  public void setStatus(Status status) {
    if (status == Status.RUNNING) {
      startTime = System.currentTimeMillis();
    } else {
      endtime = System.currentTimeMillis();
    }
    this.status = status;
  }

  public Long getStartTime() {
    return startTime;
  }

  public void setStartTime(Long startTime) {
    this.startTime = startTime;
  }

  public Long getEndtime() {
    return endtime;
  }

  public void setEndtime(Long endtime) {
    this.endtime = endtime;
  }

  public Seed getSeed() {
    return seed;
  }

  public void setSeed(Seed seed) {
    this.seed = seed;
  }
}
