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

import java.util.List;

/**
 * Collaboration service interface.
 */
public interface ICoordinate {

    /**
     *
     * @param path The specify path.
     * @return Get node value.
     */
    public String getNodeValue(String path);

    /**
     * Set node value on specify path.
     *
     * @param path The path to store.
     * @param val The value on the path.
     */
    public void setNodeValue(String path, String val);

    /**
     * Delete node.
     *
     * @param path The specify path.
     */
    public void deleteNode(String path);

    /**
     * Get children of specify path.
     *
     * @param path The path.
     * @return The children pathes.
     */
    public List<String> getChildren(String path);

    /**
     * Check the node is exist.
     *
     * @param path The specify path.
     */
    public void checkNodeExist(String path);
}
