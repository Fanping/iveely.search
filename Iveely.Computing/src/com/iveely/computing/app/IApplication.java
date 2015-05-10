package com.iveely.computing.app;

/**
 * The interface for app which develop on iveely computing.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 9:24:55
 */
public interface IApplication {

    /**
     * The status of application. Normal status:
     * JUSTINSTALL->READY->WATTING->RUNNING->SUCCESS.
     */
    public enum Status {

        /**
         * Just installed on iveely computing.
         */
        JUSTINSTALL,
        /**
         * It's ready to run.
         */
        READY,
        /**
         * Killed by user.
         */
        KILLED,
        /**
         * App has died.
         */
        DIED,
        /**
         * Will runing.
         */
        WATTING,
        /**
         * In runing.
         */
        RUNNING,
        /**
         * Finish running with success.
         */
        SUCCESS,
        /**
         * Finish running with exception.
         */
        EXCEPTION
    }

    /**
     * Execution cycle
     */
    public enum ExecutePlan {

        /**
         * Just run once.
         */
        ONCE,
        /**
         * Run at each hour.
         */
        HOURLY,
        /**
         * Run at each day.
         */
        DAILY,
        /**
         * Run at each week.
         */
        WEEKLY,
        /**
         * Run at each month.
         */
        MONTHLY,
        /**
         * Run at each minute.
         */
        ALWAYS,
        /**
         * UNKNOWN.
         */
        UNKOWN
    }
}
