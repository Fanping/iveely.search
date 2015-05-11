package com.iveely.database.api;

import com.iveely.framework.java.Convertor;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.net.Server;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.Socket;
import org.apache.log4j.Logger;

public class DbClient {

    /**
     * Server ip address.
     */
    private final String hostAddress;

    /**
     * Server port.
     */
    private final int port;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Server.class.getName());

    /**
     * Socket client.
     */
    private Socket client;

    public DbClient(String hostAddres, int port) {
        this.hostAddress = hostAddres;
        this.port = port;

    }

    /**
     * Send synchronous message.
     *
     * @param message
     * @return
     */
    public InternetPacket send(InternetPacket message) {
        try {

            try {
                this.client = new Socket(this.hostAddress, this.port);
            } catch (IOException ex) {
                return InternetPacket.getUnknowPacket();
// java.util.logging.Logger.getLogger(DbClient.class.getName()).log(Level.SEVERE, null, ex);
            }
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

    public void close() {
        if (this.client != null && !this.client.isClosed()) {
            try {
                this.client.close();
            } catch (IOException ex) {
                logger.error(ex);
            }
        }
    }
}
