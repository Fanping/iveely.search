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

import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

/**
 * Simulation of the zookeeper client,For local debugging environment.
 */
public class SimulateClient implements ICoordinate {

    /**
     * Stored data.
     */
    private final Map<String, String> data;

    /**
     * Build simulate client.
     */
    public SimulateClient() {
        this.data = new TreeMap<>();
    }

    /**
     * @see
     * com.iveely.computing.coordinate.ICoordinate#getNodeValue(java.lang.String)
     */
    @Override
    public String getNodeValue(String path) {
        if (this.data.containsKey(path)) {
            return this.data.get(path);
        }
        return null;
    }

    /**
     * @see
     * com.iveely.computing.coordinate.ICoordinate#setNodeValue(java.lang.String,
     * java.lang.String)
     */
    @Override
    public void setNodeValue(String path, String val) {
        this.data.put(path, val);
    }

    /**
     * @see
     * com.iveely.computing.coordinate.ICoordinate#deleteNode(java.lang.String)
     */
    @Override
    public void deleteNode(String path) {
        this.data.remove(path);
        List<String> children = getChildren(path);
        for (String child : children) {
            this.data.remove(path + "/" + child);
        }
    }

    /**
     * @see
     * com.iveely.computing.coordinate.ICoordinate#getChildren(java.lang.String)
     */
    @Override
    public List<String> getChildren(String path) {
        List<String> children = new ArrayList<>();
        for (Map.Entry<String, String> entrySet : data.entrySet()) {
            String key = entrySet.getKey();
            if (key.startsWith(path) && !key.equals(path)) {
                if (path.endsWith("/")) {
                    children.add(key.replace(path, ""));
                } else {
                    children.add(key.replace(path + "/", ""));
                }
            }
        }
        return children;
    }

    /**
     * @see
     * com.iveely.computing.coordinate.ICoordinate#checkNodeExist(java.lang.String)
     */
    @Override
    public void checkNodeExist(String path) {

    }

}
