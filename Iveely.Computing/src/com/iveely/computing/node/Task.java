package com.iveely.computing.node;

import com.iveely.computing.app.IApplication;
import java.util.Date;
import org.apache.log4j.Logger;

/**
 * Task in iveely.computing.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 20:42:55
 */
public class Task implements Runnable {

    /**
     * The applition which in running.
     */
    private final App app;

    /**
     * Executor of the task.
     */
    private static TaskExecutor taskExecutor;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Task.class.getName());

    public Task(App app, TaskExecutor executor) {
        this.app = app;
        taskExecutor = executor;
    }

    @Override
    public void run() {
        com.iveely.framework.java.JarExecutor executor;
        try {
            executor = new com.iveely.framework.java.JarExecutor();
            String paramVal = app.getDefaultParam();
            if (!app.getExeParam().equals("") && !app.getExeParam().equals("NULL")) {
                paramVal = app.getExeParam();
            }
            String paramValue[] = {paramVal};
            Class paramClass[] = {String.class};
            app.setExeResult(executor.invoke(app.getJarPath(), app.getExeClass(), "invoke", paramClass, paramValue).toString());
            app.setLastExeTime(new Date());
            app.setStatus(IApplication.Status.SUCCESS);
        } catch (InterruptedException e) {
            logger.error(e);
            app.setLastExeTime(new Date());
            app.setStatus(IApplication.Status.KILLED);
        } catch (Exception e) {
            logger.error(e);
            app.setException(e.toString());
            app.setLastExeTime(new Date());
            app.setStatus(IApplication.Status.EXCEPTION);
        }
        executor = null;
        taskExecutor.finish(app.getAppName());
    }

}
