package com.iveely.computing.node;

import com.iveely.computing.app.IApplication;
import java.util.Date;
import java.util.TreeMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import org.apache.log4j.Logger;

/**
 * Task executor.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 17:15:00
 */
public class TaskExecutor {

    /**
     * The list of execute.
     */
    private final TreeMap<String, Thread> executeList;

    /**
     * Thread pool.
     */
    private final ExecutorService threadPool;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(TaskExecutor.class.getName());

    public TaskExecutor() {
        executeList = new TreeMap<>();
        threadPool = Executors.newFixedThreadPool(5);
    }

    /**
     * Get current task count in running.
     *
     * @return
     */
    public int getCurrentTaskCount() {
        return executeList.size();
    }

    /**
     * Add new application to run.
     *
     * @param app
     */
    public void addApp(App app) {
        logger.info("run app:" + app.getAppName());
        app.setStatus(IApplication.Status.WATTING);
        app.setStartExeTime(new Date());
        app.setExeResult("");
        app.setException(null);
        app.setStatus(IApplication.Status.RUNNING);
        Task task = new Task(app, this);
        Thread thread = new Thread(task);
        thread.setName(app.getAppName());
        executeList.put(app.getAppName(), thread);
        // Thread passed where Runnable expected
        // A Thread object is passed as a parameter to a method where a Runnable is expected. 
        // This is rather unusual, and may indicate a logic error or cause unexpected behavior.
        threadPool.execute(thread);
    }

    /**
     * Finish runing.
     *
     * @param appName
     */
    public void finish(String appName) {
        if (executeList.containsKey(appName)) {
            executeList.remove(appName);
        }
    }

    /**
     * Remove applcaiton from list.
     *
     * @param appName
     * @return
     */
    public boolean remove(String appName) {
        if (!executeList.containsKey(appName)) {
            logger.error("remove " + appName + " not success, as not in execute list.");
            return false;
        }
        Thread thread = executeList.get(appName);
        if (!thread.isInterrupted()) {
            thread.stop();
        } else {
            logger.error("remove " + appName + " not success, as thread is not alive.");
            return false;
        }
        return true;
    }
}
