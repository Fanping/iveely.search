package com.iveely.computing.node;

import com.iveely.computing.app.IApplication;
import java.util.Calendar;
import java.util.Collection;
import java.util.Iterator;
import org.apache.log4j.Logger;

/**
 * Task schedule.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 16:58:05
 */
public class Schedule implements Runnable {

    /**
     * Max allow task count.
     */
    private final int maxTaskCount = 5;

    /**
     * the executor of task.
     */
    private final TaskExecutor executor;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Schedule.class.getName());

    private Schedule() {
        executor = new TaskExecutor();
    }

    /**
     * single instance.
     */
    private static Schedule schedule;

    /**
     * Get single instance.
     *
     * @return
     */
    public static Schedule getInstance() {
        if (schedule == null) {
            schedule = new Schedule();
        }
        return schedule;
    }

    /**
     * Add new application.
     *
     * @param app
     */
    public void addNewApp(App app) {
        executor.addApp(app);
    }

    /**
     * Kill application if in runing.
     *
     * @param appName
     * @return
     */
    public boolean killApp(String appName) {
        return executor.remove(appName);
    }

    @Override
    public void run() {
        while (true) {
            try {
                // 1. Get all applications.
                Collection<App> apps = Attribute.getInstance().getApplications();
                Iterator<App> it = apps.iterator();
                while (it.hasNext()) {
                    if (executor.getCurrentTaskCount() < maxTaskCount + 1) {
                        App app = it.next();

                        // 2. If match.
                        IApplication.Status status = app.getStatus();
                        if (status != IApplication.Status.JUSTINSTALL
                                && status != IApplication.Status.RUNNING
                                && status != IApplication.Status.DIED) {

                            // 3.1 If just run once.
                            if (app.getExecutePlan() == IApplication.ExecutePlan.ONCE) {
                                if (status == IApplication.Status.JUSTINSTALL) {
                                    addNewApp(app);
                                }
                                continue;
                            }

                            // 3.2 If run hourly.
                            if (app.getExecutePlan() == IApplication.ExecutePlan.HOURLY) {
                                long diff = Calendar.getInstance().getTime().getTime() - app.getLastExeTime().getTime();
                                long hour = diff / (1000 * 60 * 60);
                                if (hour >= 1) {
                                    addNewApp(app);
                                }
                                continue;
                            }

                            // 3.2 If run daily.
                            if (app.getExecutePlan() == IApplication.ExecutePlan.DAILY) {
                                long diff = Calendar.getInstance().getTime().getTime() - app.getLastExeTime().getTime();
                                long days = diff / (1000 * 60 * 60 * 24);
                                if (days >= 1) {
                                    addNewApp(app);
                                }
                                continue;
                            }

                            // 3.3 If run weekly.
                            if (app.getExecutePlan() == IApplication.ExecutePlan.WEEKLY) {
                                long diff = Calendar.getInstance().getTime().getTime() - app.getLastExeTime().getTime();
                                long weeks = diff / (1000 * 60 * 60 * 24 * 7);
                                if (weeks >= 1) {
                                    addNewApp(app);
                                }
                                continue;
                            }

                            // 3.4 If run monthly.
                            if (app.getExecutePlan() == IApplication.ExecutePlan.MONTHLY) {
                                long diff = Calendar.getInstance().getTime().getTime() - app.getLastExeTime().getTime();
                                long monthes = diff / (1000 * 60 * 60 * 24 * 7 * 30);
                                if (monthes >= 1) {
                                    addNewApp(app);
                                }
                                continue;
                            }

                            // 3.5 If always keep running.
                            if (app.getExecutePlan() == IApplication.ExecutePlan.ALWAYS) {
                                addNewApp(app);
                            }
                        }
                    } else {
                        break;
                    }
                }
                Thread.sleep(1000 * 60);
            } catch (InterruptedException ex) {
                logger.error(ex);
            }
        }
    }
}
