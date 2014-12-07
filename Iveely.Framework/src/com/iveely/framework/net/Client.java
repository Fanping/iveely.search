package com.iveely.framework.net;

import com.iveely.framework.database.Convertor;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import org.apache.log4j.Logger;

/**
 * Net connector - client.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 10:57:58
 */
public class Client {

    /**
     * Server ip address.
     */
    private final String hostAddress;

    /**
     * Server port.
     */
    private int port;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Server.class.getName());

    public Client(String hostAddres, int port) {
        this.hostAddress = hostAddres;
        this.port = port;
        if (port > 0 && port < 65535) {
            this.port = port;
        } else {
            logger.error(port + " is not in 0~65535");
            this.port = -1;
        }
    }

    /**
     * Send synchronous message.
     *
     * @param message
     * @return
     */
    public InternetPacket send(InternetPacket message) {
        try {
            Socket client = new Socket(this.hostAddress, this.port);

            // 1. Prepare.
            OutputStream outputStream = client.getOutputStream();

            // 2. Data convert.
            byte[] bytes = message.toBytes();
            byte[] lengthBytes = Convertor.int2byte(bytes.length, 4);
            outputStream.write(lengthBytes);
            outputStream.write(bytes);

            // 3. Response message.
            InputStream inputStream = client.getInputStream();
            int readySize = inputStream.read(lengthBytes, 0, 4);
            while (readySize < 4) {
                readySize += inputStream.read(lengthBytes, readySize, 4 - readySize);
            }
            int offset = Convertor.bytesToInt(lengthBytes);
            byte[] feedbackBytes = new byte[offset];
            int readCount = 0;
            while (readCount < offset) {
                readCount += inputStream.read(feedbackBytes, readCount, offset - readCount);
            }
            InternetPacket feedbackPacket = new InternetPacket();
            feedbackPacket = feedbackPacket.toIPacket(feedbackBytes);
            return feedbackPacket;

        } catch (IOException e) {
            logger.error(e);
        }
        return InternetPacket.getUnknowPacket();
    }
}
