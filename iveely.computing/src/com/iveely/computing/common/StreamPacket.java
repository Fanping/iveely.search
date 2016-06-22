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

import java.io.Serializable;
import org.apache.commons.lang.SerializationUtils;
import org.apache.log4j.Logger;

/**
 *
 * @author Iveely Liu
 */
public class StreamPacket implements Serializable {

    private static final long serialVersionUID = -8902317L;

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(StreamPacket.class.getName());

    /**
     * Guid of the topology.
     */
    private String guid;

    /**
     * @return the guid
     */
    public String getGuid() {
        return guid;
    }

    /**
     * @param guid the guid to set
     */
    public void setGuid(String guid) {
        this.guid = guid;
    }

    /**
     * Name of the topology.
     */
    private String name;

    /**
     * @return the name
     */
    public String getName() {
        return name;
    }

    /**
     * @param name the name to set
     */
    public void setName(String name) {
        this.name = name;
    }

    /**
     * Stream commond type.
     */
    private StreamType type;

    /**
     * @return the type
     */
    public StreamType getType() {
        return type;
    }

    /**
     * @param type the type to set
     */
    public void setType(StreamType type) {
        this.type = type;
    }

    /**
     * Stream data.
     */
    private byte[] data;

    /**
     * @return the data
     */
    public byte[] getData() {
        byte[] copyVersion = data;
        return copyVersion;
    }

    /**
     * @param data the data to set
     */
    public void setData(byte[] data) {
        this.data = (byte[]) data.clone();
    }

    /**
     * Stream packet to bytes.
     *
     * @return
     */
    public byte[] toBytes() {
        return SerializationUtils.serialize(this);
    }

    /**
     * Bytes to stream packet.
     *
     * @param bytes
     * @return
     */
    public StreamPacket toObject(byte[] bytes) {
        return (StreamPacket) SerializationUtils.deserialize(bytes);
    }
}
