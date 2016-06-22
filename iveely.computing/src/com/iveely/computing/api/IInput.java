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
package com.iveely.computing.api;

import com.iveely.computing.coordinate.Coordinator;
import java.util.HashMap;
import java.util.UUID;
import org.apache.log4j.Logger;

/**
 * Data Input.It is the input source data, can be gained by reading the file
 * system data source, also can be achieved by other ways.
 *
 * @author Iveely Liu
 */
public abstract class IInput {

    /**
     * Name of the data-input.
     */
    private final String name;

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(IInput.class);

    /**
     * Build IInput instance.
     */
    public IInput() {
        this.name = this.getClass().getSimpleName() + "(" + UUID.randomUUID().toString() + ")";
    }

    /**
     * Prepare before execute, initialization method. You can customize
     * functionality. Including initialization, can be regarded as a
     * constructor.
     *
     * @param conf The user's custom configuration information.
     */
    public void start(HashMap<String, Object> conf) {
    }

    /**
     * Next data.
     *
     * @param channel Stream channel.
     */
    public abstract void nextTuple(StreamChannel channel);

    /**
     * Data to which output. The next step in the data transfer to another
     * IOuput.
     *
     * @param channel Stream channel.
     */
    public abstract void toOutput(StreamChannel channel);

    /**
     *
     * @return Name of the task.
     */
    public String getName() {
        return name;
    }

    /**
     * Finally function after the nexTuple(StreamChanel channel) method, can be
     * regarded as the destructor.
     *
     * @param conf The user's custom configuration information.
     */
    public void end(HashMap<String, Object> conf) {
    }

    /**
     * Set public cache. Everyone can access it. actually store on zookeeper.
     *
     * @param key The key of cache.
     * @param value The value of cache.
     */
    public void setPublicCache(String key, String value) {
        Coordinator.getInstance().setNodeValue("/cache/" + key, value);
    }

    /**
     * Get public cache. Everyone can get it.
     *
     * @param key The key of cache.
     * @return The cache value.
     */
    public String getPublicCache(String key) {
        return Coordinator.getInstance().getNodeValue("/cache/" + key);
    }
}
