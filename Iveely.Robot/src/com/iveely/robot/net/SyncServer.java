/**
 * date   : 2016年1月31日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.net;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.util.concurrent.ExecutorService;

import org.apache.log4j.Logger;

import com.iveely.robot.util.Convertor;

/**
 * @author {Iveely Liu}
 *
 */
public class SyncServer {

	/**
	 * Message call back.
	 *
	 * @author liufanping@iveely.com
	 */
	public interface ICallback {

		/**
		 * call back method.
		 *
		 * @param packet
		 *            InternetPacket
		 * @return
		 */
		public Packet invoke(Packet packet);
	}

	/**
	 * Server executor.
	 *
	 * @author liufanping@iveely.com
	 * @date 2014-10-18 11:23:11
	 */
	private class Executor extends Thread {

		/**
		 * The client socket.
		 */
		private final Socket socket;

		/**
		 * The message call back.
		 */
		private final ICallback callback;

		/**
		 * Logger.
		 */
		private final Logger logger = Logger.getLogger(Executor.class);

		public Executor(Socket socket, ICallback callback) {
			this.socket = socket;
			this.callback = callback;
		}

		@Override
		public void run() {
			try {
				InputStream in = this.socket.getInputStream();
				OutputStream out = this.socket.getOutputStream();
				while (true) {
					// 1. Get message.
					byte[] receivBufLength = new byte[4];
					int readySize = in.read(receivBufLength, 0, 4);
					while (readySize < 4) {
						readySize += in.read(receivBufLength, readySize, 4 - readySize);
					}
					int reviceSize = Convertor.bytesToInt(receivBufLength);
					byte[] receivBuf = new byte[reviceSize];
					int readCount = 0;
					while (readCount < reviceSize) {
						readCount += in.read(receivBuf, readCount, reviceSize - readCount);
					}
					Packet packet = new Packet();
					packet = packet.toPacket(receivBuf);
					if (packet == null) {
						break;
					}
					if (packet.getExecutType() != -1000) {
						// 2. Process message.
						packet = this.callback.invoke(packet);
					}

					// 3. Response message.
					byte[] bytes = packet.toBytes();
					byte[] feedbackSizeBytes = Convertor.int2byte(bytes.length, 4);
					out.write(feedbackSizeBytes);
					out.write(bytes);
					out.flush();
				}
				// this.socket.close();
			} catch (IOException e) {
				logger.error(e);
			}
		}
	}

	/**
	 * Client count.
	 */
	private final int currentClientCount;

	/**
	 * Multithreading lock object.
	 */
	private static Object threadObj;

	/**
	 * Max client count.
	 */
	private final int MAX_CLIENT_COUNT = 1024;

	/**
	 * Service port.
	 */
	private final int port;

	/**
	 * Call back of the message.
	 */
	private ICallback callback;

	/**
	 * Logger
	 */
	private final Logger logger = Logger.getLogger(SyncServer.class);

	/**
	 * Executor service.
	 */
	private ExecutorService executorService;

	public SyncServer(ICallback callback, int port) {
		currentClientCount = 0;
		threadObj = new Object();
		if (port > 0 && port < 65535) {
			this.port = port;
		} else {
			logger.error(port + " is not in 0~65535");
			this.port = -1;
		}
		if (callback != null) {
			this.callback = callback;
		} else {
			logger.error("Call back function can not be null.");
		}
	}

	/**
	 * Start service.
	 */
	public void start() {
		try {
			ServerSocket serverSocket = new ServerSocket(port);
			// logger.info("sss");
			while (true) {
				Socket socket = serverSocket.accept();
				Executor executor = new Executor(socket, callback);
				executor.start();
			}
		} catch (IOException ex) {
			ex.printStackTrace();
		}
	}

	/**
	 * Get the service of port.
	 *
	 * @return
	 */
	public int getPort() {
		return this.port;
	}
}
