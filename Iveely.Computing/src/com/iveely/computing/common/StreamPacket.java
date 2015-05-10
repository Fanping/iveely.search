package com.iveely.computing.common;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.Serializable;
import org.apache.log4j.Logger;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-6 23:03:09
 */
public class StreamPacket implements Serializable {

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
        this.data = data;
    }

    /**
     * Stream packet to bytes.
     *
     * @return
     */
    public byte[] toBytes() {
        try {
            ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
            try (ObjectOutputStream objectOutputStream = new ObjectOutputStream(byteArrayOutputStream)) {
                objectOutputStream.writeObject(this);
                objectOutputStream.flush();
            }
            return byteArrayOutputStream.toByteArray();
        } catch (Exception e) {
            logger.error(e);
        }
        return null;
    }

    /**
     * Bytes to stream packet.
     *
     * @param bytes
     * @return
     */
    public StreamPacket toObject(byte[] bytes) {
        try {
            ByteArrayInputStream byteArrayInputStream = new ByteArrayInputStream(bytes);
            ObjectInputStream objectInputStream = new ObjectInputStream(byteArrayInputStream);
            StreamPacket packet = (StreamPacket) objectInputStream.readObject();
            return packet;
        } catch (IOException | ClassNotFoundException e) {
            logger.error(e);
        }
        return null;
    }
}
