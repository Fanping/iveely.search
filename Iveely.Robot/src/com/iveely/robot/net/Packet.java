/**
 * date   : 2016年1月31日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.net;

import org.apache.log4j.Logger;

import com.iveely.robot.util.Convertor;

import bsh.StringUtil;

/**
 * @author {Iveely Liu}
 *
 */
public class Packet {

	/**
	 * Mime type in tranfer.
	 * 
	 * @author {Iveely Liu}
	 *
	 */
	public enum MimeType {
		INTERGE, DOUBLE, BOOLEAN, STRING, OBJECT, JSON,
	}

	/**
	 * Logger.
	 */
	private static final Logger logger = Logger.getLogger(Packet.class);

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
	 * @param mimeType
	 *            the mimeType to set
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
	 * @param executType
	 *            the executType to set
	 */
	public void setExecutType(int executType) {
		this.executeType = executType;
	}

	/**
	 * The context of packet.
	 */
	private byte[] data;

	/**
	 * Set data with byte[].
	 *
	 * @param data
	 */
	public void setData(byte[] data) {
		this.data = data;
	}

	/**
	 * Set data with type of string.
	 * 
	 * @param data
	 */
	public void setData(String data) {
		this.data = com.iveely.robot.util.StringUtil.getBytes(data);
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
		// May expose internal representation by returning reference to mutable
		// object.
		return this.data;
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
	public Packet toPacket(byte[] bytes) {
		if (bytes.length == 0) {
			// close system cmd.
			return null;
		}
		if (bytes.length < 8) {
			return Packet.getUnknowPacket();
		}
		try {
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
		} catch (Exception e) {
			logger.error(e);
		}
		return getUnknowPacket();
	}

	/**
	 * Unknown packet.
	 *
	 * @return
	 */
	public static Packet getUnknowPacket() {
		Packet unknowPacket = new Packet();
		unknowPacket.setExecutType(999);
		unknowPacket.setMimeType(999);
		unknowPacket.setData("Unknow".getBytes());
		return unknowPacket;
	}
}
