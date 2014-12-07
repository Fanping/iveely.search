package com.iveely.framework.net;

import com.iveely.framework.database.Convertor;

/**
 * Internet packet.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 19:17:38
 */
public class InternetPacket {

    /**
     * Mime type.
     */
    private int mimeType;

    /**
     * Get mime type.
     *
     * @return the mimeType
     */
    public int getMimeType() {
        return this.mimeType;
    }

    /**
     * Set mime type.
     *
     * @param mimeType the mimeType to set
     */
    public void setMimeType(int mimeType) {
        this.mimeType = mimeType;
    }

    /**
     * Execute type.
     */
    private int executeType;

    /**
     * Get execute type.
     *
     * @return the executType
     */
    public int getExecutType() {
        return this.executeType;
    }

    /**
     * Execute type.
     *
     * @param executType the executType to set
     */
    public void setExecutType(int executType) {
        this.executeType = executType;
    }

    /**
     * The context of packet.
     */
    private byte[] data;

    /**
     * Set data.
     *
     * @param data
     */
    public void setData(byte[] data) {
        this.data = data;
    }

    /**
     * Get data.
     *
     * @return
     */
    public byte[] getData() {
        if (this.data == null) {
            return null;
        }
        // May expose internal representation by returning reference to mutable object.
        byte[] bytes = this.data;
        return bytes;
    }

    /**
     * Internet packet to bytes.
     *
     * @return
     */
    public byte[] toBytes() {
        byte[] bytes = new byte[getData().length + 8];
        byte[] executeTypeBytes = Convertor.int2byte(getExecutType(), 4);
        byte[] mimeTypeBytes = Convertor.int2byte(getMimeType(), 4);
        System.arraycopy(executeTypeBytes, 0, bytes, 0, 4);
        System.arraycopy(mimeTypeBytes, 0, bytes, 4, 4);
        System.arraycopy(getData(), 0, bytes, 8, getData().length);
        return bytes;
    }

    /**
     * Convert bytes to Internet packet.
     *
     * @param bytes
     * @return
     */
    public InternetPacket toIPacket(byte[] bytes) {
        byte[] executeTypeBytes = new byte[4];
        System.arraycopy(bytes, 0, executeTypeBytes, 0, 4);
        int exeType = Convertor.bytesToInt(executeTypeBytes);
        setExecutType(exeType);

        byte[] mimeTypeBytes = new byte[4];
        System.arraycopy(bytes, 4, mimeTypeBytes, 0, 4);
        int mType = Convertor.bytesToInt(mimeTypeBytes);
        setMimeType(mType);

        byte[] dataBytes = new byte[bytes.length - 8];
        System.arraycopy(bytes, 8, dataBytes, 0, dataBytes.length);
        setData(dataBytes);
        return this;
    }

    /**
     * Unknown packet.
     *
     * @return
     */
    public static InternetPacket getUnknowPacket() {
        InternetPacket unknowPacket = new InternetPacket();
        unknowPacket.setExecutType(999);
        unknowPacket.setMimeType(999);
        unknowPacket.setData("Unknow".getBytes());
        return unknowPacket;
    }
}
