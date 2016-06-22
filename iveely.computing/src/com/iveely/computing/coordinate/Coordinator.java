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
package com.iveely.computing.coordinate;

import com.iveely.computing.config.SystemConfig;

/**
 * Coordinator to store data of environment.
 */
public class Coordinator {

    /**
     * Instance of coordinator.
     */
    private static ICoordinate ic;

    /**
     *
     * @return Get coordinator instance.
     */
    public static ICoordinate getInstance() {
        if (ic == null) {
            if (SystemConfig.isCluster) {
                ic = ZookeeperClient.getInstance();
            } else {
                ic = new SimulateClient();
            }
        }
        return ic;
    }
}
