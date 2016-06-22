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
package com.iveely.computing.user;

import com.iveely.computing.common.Message;
import com.iveely.computing.config.ConfigWrapper;
import com.iveely.framework.net.AsynClient;
import com.iveely.framework.net.Packet;
import com.iveely.framework.text.Convertor;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.Charset;
import java.util.Locale;
import org.apache.log4j.Logger;

/**
 * Console to accept user's commands.
 */
public class Console implements Runnable {

    /**
     * Connector.
     */
    private final AsynClient client;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Console.class.getName());

    /**
     * Build console instance.
     */
    public Console() {
        client = new AsynClient(ConfigWrapper.get().getMaster().getAddress(), ConfigWrapper.get().getMaster().getPort(), new Callback());
    }

    /**
     * Run console.
     */
    @Override
    public void run() {
        Thread.currentThread().setName("console thread");
        while (true) {
            System.out.print("Command:");
            try {
                InputStreamReader reader = new InputStreamReader(System.in, Charset.defaultCharset());
                BufferedReader inputReader = new BufferedReader(reader);
                String input = inputReader.readLine();
                String[] cmds = input.split(" ");
                logger.info("Get command:" + input);
                processCmd(cmds);
            } catch (IOException e) {
                logger.error("When write some commands on slave,exception happend.", e);
            }
        }
    }

    /**
     * Process command from user.
     *
     * @param cmds
     * @throws FileNotFoundException
     * @throws IOException
     */
    public void processCmd(String[] cmds) throws FileNotFoundException, IOException {
        Message.ExecuteType executeType = Message.ExecuteType.valueOfName(cmds[0].toUpperCase(Locale.CHINESE));
        if (executeType != Message.ExecuteType.UNKOWN && executeType != Message.ExecuteType.UPLOAD) {
            Message.MIMEType mimeType = Message.MIMEType.MESSAGE;
            StringBuilder infor = new StringBuilder();
            for (int i = 1; i < cmds.length; i++) {
                infor.append(cmds[i]).append(" ");
                mimeType = Message.MIMEType.TEXT;
            }
            Packet packet = new Packet();
            packet.setExecuteType(executeType.ordinal());
            packet.setMimeType(mimeType.ordinal());
            packet.setData(Message.getBytes(infor.toString().trim()));
            client.send(packet);
        } else if (Message.ExecuteType.UPLOAD == executeType) {
            if (cmds.length < 3) {
                logger.error("Upload application should be specify app folder.");
                return;
            }

            // 1. Check file exist.
            String fileName = cmds[1];
            File file = new File(fileName);
            if (!file.exists()) {
                logger.error("The specify app folder is not exist.");
                return;
            }
            logger.info("Check file exist: pass.");

            ByteArrayOutputStream out;
            try ( // 2. Convert to byte[].
                    BufferedInputStream inputStream = new BufferedInputStream(new FileInputStream(fileName))) {
                out = new ByteArrayOutputStream(inputStream.available());
                byte[] temp = new byte[inputStream.available()];
                int size;
                while ((size = inputStream.read(temp)) != -1) {
                    out.write(temp, 0, size);
                }
            }
            byte[] content = out.toByteArray();

            // 3. Build packet.
            Packet packet = new Packet();
            packet.setExecuteType(Message.ExecuteType.UPLOAD.ordinal());
            packet.setMimeType(Message.MIMEType.APP.ordinal());
            byte[] appName = Message.getBytes(cmds[2]);
            byte[] appNameSize = Convertor.int2byte(appName.length);
            byte[] data = new byte[4 + appName.length + content.length];
            System.arraycopy(appNameSize, 0, data, 0, 4);
            System.arraycopy(appName, 0, data, 4, appName.length);
            System.arraycopy(content, 0, data, 4 + appName.length, content.length);
            packet.setData(data);
            client.send(packet);
        } else {
            logger.error("Unknow execute type:" + cmds[0]);
        }
    }
}
