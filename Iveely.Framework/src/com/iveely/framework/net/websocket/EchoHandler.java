package com.iveely.framework.net.websocket;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import org.apache.log4j.Logger;

/**
 * Websocket echo handler.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-2 22:27:09
 */
public class EchoHandler implements Runnable {

    /**
     * Socket.
     */
    private final Socket socket;

    /**
     * Hand shake.
     */
    private boolean hasHandshake = false;

    /**
     * Charset.
     */
    private final Charset charset;

    /**
     * logger.
     */
    private final Logger logger = Logger.getLogger(EchoHandler.class.getName());

    private final IEventProcessor processor;

    public EchoHandler(Socket socket, IEventProcessor processor) {
        this.charset = Charset.forName("UTF-8");
        this.socket = socket;
        this.processor = processor;
    }

    /**
     * Get writer.
     *
     * @param socket
     * @return
     * @throws IOException
     */
    private PrintWriter getWriter(Socket socket) throws IOException {
        OutputStream socketOut = socket.getOutputStream();
        return new PrintWriter(socketOut, true);
    }

    /**
     * Process message.
     *
     * @param msg
     * @return
     */
    public String processMessage(String msg) {
        logger.info("Get message:" + msg);
        return this.processor.invoke(msg);
    }

    /**
     * Execute.
     */
    @Override
    public void run() {
        try {
            logger.info("New connection accepted" + socket.getInetAddress() + ":" + socket.getPort());
            try (InputStream inputStream = socket.getInputStream()) {
                PrintWriter printWriter = getWriter(socket);
                byte[] buf = new byte[1024];
                int len = inputStream.read(buf, 0, 1024);
                byte[] res = new byte[len];
                System.arraycopy(buf, 0, res, 0, len);
                String key = new String(res);
                if (!hasHandshake && key.indexOf("Key") > 0) {
                    // Hand shake
                    String keyVal = key;
                    try {
                        key = key.substring(0, key.indexOf("==") + 2);
                        key = key.substring(key.indexOf("Key") + 4, key.length()).trim();
                    } catch (Exception e) {
                        logger.error(keyVal);
                    }

                    key += "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                    MessageDigest md = MessageDigest.getInstance("SHA-1");
                    md.update(key.getBytes("utf-8"), 0, key.length());
                    byte[] sha1Hash = md.digest();
                    sun.misc.BASE64Encoder encoder = new sun.misc.BASE64Encoder();
                    key = encoder.encode(sha1Hash);
                    printWriter.println("HTTP/1.1 101 Switching Protocols");
                    printWriter.println("Upgrade: websocket");
                    printWriter.println("Connection: Upgrade");
                    printWriter.println("Sec-WebSocket-Accept: " + key);
                    printWriter.println();
                    printWriter.flush();
                    hasHandshake = true;
                    byte[] first = new byte[1];
                    int read = inputStream.read(first, 0, 1);
                    while (read > 0) {
                        int b = first[0] & 0xFF;
                        //1 means data，8 is close socket.
                        byte opCode = (byte) (b & 0x0F);

                        if (opCode == 8) {
                            socket.getOutputStream().close();
                            break;
                        }
                        b = inputStream.read();
                        int payloadLength = b & 0x7F;
                        if (payloadLength == 126) {
                            byte[] extended = new byte[2];
                            inputStream.read(extended, 0, 2);
                            int shift = 0;
                            payloadLength = 0;
                            for (int i = extended.length - 1; i >= 0; i--) {
                                payloadLength = payloadLength + ((extended[i] & 0xFF) << shift);
                                shift += 8;
                            }

                        } else if (payloadLength == 127) {
                            byte[] extended = new byte[8];
                            inputStream.read(extended, 0, 8);
                            int shift = 0;
                            payloadLength = 0;
                            for (int i = extended.length - 1; i >= 0; i--) {
                                payloadLength = payloadLength + ((extended[i] & 0xFF) << shift);
                                shift += 8;
                            }
                        }

                        byte[] mask = new byte[4];
                        inputStream.read(mask, 0, 4);
                        int readThisFragment = 1;
                        ByteBuffer byteBuf = ByteBuffer.allocate(payloadLength + 10);
                        while (payloadLength > 0) {
                            int masked = inputStream.read();
                            masked = masked ^ (mask[(int) ((readThisFragment - 1) % 4)] & 0xFF);
                            byteBuf.put((byte) masked);
                            payloadLength--;
                            readThisFragment++;
                        }
                        byteBuf.flip();
                        String userQuery = getUserQuery(byteBuf.array());
                        String result = processMessage(userQuery);
                        byte[] respBytes = result.getBytes("UTF-8");
                        byteBuf.clear();
                        byteBuf = ByteBuffer.allocate(respBytes.length + 10);
                        byteBuf.put(respBytes);
                        responseClient(byteBuf, true);
                        inputStream.read(first, 0, 1);
                    }

                }
            }
        } catch (IOException | NoSuchAlgorithmException e) {
            logger.error(e);
        } finally {
            try {
                if (socket != null) {
                    socket.close();
                }
            } catch (IOException e) {
                logger.error(e);
            }
        }
    }

    /**
     * Response client.
     *
     * @param byteBuf
     * @param finalFragment
     * @throws IOException
     */
    private void responseClient(ByteBuffer byteBuf, boolean finalFragment) throws IOException {
        OutputStream out = socket.getOutputStream();
        int first = 0x00;
        if (finalFragment) {
            first = first + 0x80;
            first = first + 0x1;
        }
        out.write(first);

        if (byteBuf.limit() < 126) {
            out.write(byteBuf.limit());
        } else if (byteBuf.limit() < 65536) {
            out.write(126);
            out.write(byteBuf.limit() >>> 8);
            out.write(byteBuf.limit() & 0xFF);
        } else {
            out.write(127);
            out.write(0);
            out.write(0);
            out.write(0);
            out.write(0);
            out.write(byteBuf.limit() >>> 24);
            out.write(byteBuf.limit() >>> 16);
            out.write(byteBuf.limit() >>> 8);
            out.write(byteBuf.limit() & 0xFF);

        }

        // Write the content
        out.write(byteBuf.array(), 0, byteBuf.limit());
        out.flush();
    }

    private String getUserQuery(byte[] array) {
        ByteArrayInputStream byteIn = new ByteArrayInputStream(array);
        InputStreamReader reader = new InputStreamReader(byteIn, charset.newDecoder());
        int b = 0;
        String res = "";
        try {
            while ((b = reader.read()) > 0) {
                res += (char) b;
            }
        } catch (IOException e) {
            logger.error(e);
        }
        return res;
    }
}
