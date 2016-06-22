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
package com.iveely.computing.host;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

/**
 * The slave (one) with the topolgy information.
 */
public class SlaveTopology {

    /**
     * Slave topology instance.
     */
    private static SlaveTopology instance;

    /**
     * The relation bettwen slave and topologies.
     */
    private static Map<String, HashSet<String>> list;

    /**
     * Build slave topology.
     */
    private SlaveTopology() {

    }

    /**
     * Get single instance.
     *
     * @return The slave toplogy.
     */
    public static SlaveTopology getInstance() {
        if (instance == null) {
            instance = new SlaveTopology();
        }
        return instance;
    }

    /**
     * Record toplogy on a slave.
     *
     * @param node Slave name.
     * @param tpName Topology name.
     */
    public void set(String node, String tpName) {
        if (list == null) {
            list = new HashMap<>();
        }
        if (list.containsKey(node)) {
            if (!list.get(node).contains(tpName)) {
                list.get(node).add(tpName);
            }
        } else {
            HashSet<String> set = new HashSet<>();
            set.add(tpName);
            list.put(node, set);
        }
    }

    /**
     * Get toplogyies on a slave.
     *
     * @param node
     * @return
     */
    public String[] get(String node) {
        if (list.containsKey(node)) {
            Set<String> set = list.get(node);
            String[] res = new String[set.size()];
            res = set.toArray(res);
            return res;
        }
        return new String[0];
    }
}
