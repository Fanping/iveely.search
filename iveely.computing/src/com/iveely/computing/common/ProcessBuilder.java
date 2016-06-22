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
package com.iveely.computing.common;

import org.apache.log4j.Logger;

/**
 * Build new Java process.
 */
public class ProcessBuilder {

    private static final Logger logger = Logger.getLogger(ProcessBuilder.class);

    /**
     * Start a java process by jar.
     *
     * @param jarPath The jar to run.
     * @param args The arguments to the main function of the jar.
     * @return The process information.
     */
    public static Process start(String jarPath, String args) {
        Runtime rt = Runtime.getRuntime();
        Process p;
        String fileLac;
        try {
            fileLac = "java -Djava.ext.dirs=lib/ -jar -Xms32m -Xmx1724m " + jarPath + " " + args;
            logger.info(String.format("start process with commond %s.", fileLac));
            p = rt.exec(fileLac);
            return p;
        } catch (Exception e) {
            logger.error("Process start failure.", e);
        }
        return null;
    }

    /**
     * Kill a process.
     *
     * @param p The process handle.
     */
    public static void kill(Process p) {
        if (p != null && p.isAlive()) {
            p.destroy();
        }
    }
}
