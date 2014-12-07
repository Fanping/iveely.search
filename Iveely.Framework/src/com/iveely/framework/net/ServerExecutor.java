package com.iveely.framework.net;

import com.iveely.framework.database.Convertor;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import org.apache.log4j.Logger;

/**
 * Server executor.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 11:23:11
 */
public class ServerExecutor extends Thread {

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
    private final Logger logger = Logger.getLogger(ServerExecutor.class.getName());

    public ServerExecutor(Socket socket, ICallback callback) {
        this.socket = socket;
        this.callback = callback;
    }

    @Override
    public void run() {
        try {
            InputStream in = this.socket.getInputStream();
            OutputStream out = this.socket.getOutputStream();

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
            InternetPacket packet = new InternetPacket();
            packet = packet.toIPacket(receivBuf);

            // 2. Process message.
            InternetPacket feedbackData = this.callback.invoke(packet);

            // 3. Response message.
            byte[] bytes = feedbackData.toBytes();
            byte[] feedbackSizeBytes = Convertor.int2byte(bytes.length, 4);
            out.write(feedbackSizeBytes);
            out.write(bytes);
            this.socket.close();
        } catch (IOException e) {
            logger.error(e);
        } finally {
            if (this.socket.isConnected()) {
                try {
                    this.socket.close();
                } catch (IOException e) {
                    logger.error(e);
                }
            }
        }
    }
}
