/*
 * Copyright 2016 liufanping@iveely.com.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.iveely.computing.app;

/**
 * The interface for app which develop on iveely computing.
 *
 * @author Iveely Liu
 */
public interface IApplication {

    /**
     * The status of application. Normal status:
     * JUSTINSTALL READY WATTING RUNNING SUCCESS.
     */
    public enum Status {

        /**
         * Just installed on iveely computing.
         */
        JUSTINSTALL, /**
         * It's ready to run.
         */
        READY, /**
         * Killed by user.
         */
        KILLED, /**
         * App has died.
         */
        DIED, /**
         * Will runing.
         */
        WATTING, /**
         * In runing.
         */
        RUNNING, /**
         * Finish running with success.
         */
        SUCCESS, /**
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
        ONCE, /**
         * Run at each hour.
         */
        HOURLY, /**
         * Run at each day.
         */
        DAILY, /**
         * Run at each week.
         */
        WEEKLY, /**
         * Run at each month.
         */
        MONTHLY, /**
         * Run at each minute.
         */
        ALWAYS, /**
         * UNKNOWN.
         */
        UNKOWN
    }
}
