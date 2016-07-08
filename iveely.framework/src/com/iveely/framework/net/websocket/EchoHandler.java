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
package com.iveely.framework.net.websocket;

import com.iveely.framework.text.StringUtil;

import org.apache.log4j.Logger;

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

/**
 * Websocket echo handler.
 *
 * @author liufanping (liufanping@iveely.com)
 */
public class EchoHandler implements Runnable {

  /**
   * Socket.
   */
  private final Socket socket;
  /**
   * Charset.
   */
  private final Charset charset;
  /**
   * logger.
   */
  private final Logger logger = Logger.getLogger(EchoHandler.class);
  private final SocketServer.IHandler processor;
  /**
   * Hand shake.
   */
  private boolean hasHandshake = false;

  public EchoHandler(Socket socket, SocketServer.IHandler processor) {
    this.charset = Charset.forName("UTF-8");
    this.socket = socket;
    this.processor = processor;
  }

  /**
   * Get writer.
   */
  private PrintWriter getWriter(Socket socket) throws IOException {
    OutputStream socketOut = socket.getOutputStream();
    return new PrintWriter(socketOut, true);
  }

  /**
   * Process message.
   */
  public String processMessage(String msg) {
    logger.info("Get message:" + msg);
    return this.processor.invoke(-1, null, msg);
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
        String key = StringUtil.getString(res);
        int keyposition = key.indexOf("Key");
        if (!hasHandshake && keyposition > 0 && keyposition < key.length()) {
          // Hand shake
          String keyVal = key;
          try {
            key = key.substring(0, key.indexOf("==") + 2);
            key = key.substring(key.indexOf("Key") + 4, key.length()).trim();
          } catch (Exception e) {
            logger.error(keyVal, e.getCause());
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
            // 1 means data is close socket.
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
    int b;
    StringBuilder res = new StringBuilder();
    try {
      while ((b = reader.read()) > 0) {
        res.append((char) b);
      }
    } catch (IOException e) {
      logger.error(e);
    }
    return res.toString();
  }
}
