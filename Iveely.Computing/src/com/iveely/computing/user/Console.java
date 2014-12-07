package com.iveely.computing.user;

import com.iveely.computing.common.Message;
import com.iveely.computing.config.Configurator;
import com.iveely.framework.database.Convertor;
import com.iveely.framework.file.Zip;
import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;

/**
 * Console to accept user's commands.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 11:24:33
 */
public class Console implements Runnable {

    /**
     * Connector.
     */
    private final Client client;

    public Console() {
        client = new Client(Configurator.getMasterAddress(), Configurator.getMasterPort());
    }

    @Override
    public void run() {
        while (true) {
            System.out.print("Command：");
            try {
                InputStreamReader reader = new InputStreamReader(System.in);
                BufferedReader inputReader = new BufferedReader(reader);
                String input = inputReader.readLine();
                String[] cmds = input.split(" ");
                Message.ExecutType executType = Message.ExecutType.valueOfName(cmds[0].toUpperCase());
                if (executType != Message.ExecutType.UNKOWN && executType != Message.ExecutType.UPLOAD) {
                    Message.MIMEType mimeType = Message.MIMEType.MESSAGE;
                    StringBuilder infor = new StringBuilder();
                    for (int i = 1; i < cmds.length; i++) {
                        infor.append(cmds[i]).append(" ");
                        mimeType = Message.MIMEType.TEXT;
                    }
                    InternetPacket packet = new InternetPacket();
                    packet.setExecutType(executType.ordinal());
                    packet.setMimeType(mimeType.ordinal());
                    packet.setData(Message.getBytes(infor.toString().trim()));
                    packet = client.send(packet);
                    Message.MIMEType respMimeType = Message.getMIMEType(packet.getMimeType());
                    if (respMimeType == Message.MIMEType.MESSAGE) {
                        System.out.println("Execute success.");
                    } else if (respMimeType == Message.MIMEType.TEXT) {
                        System.out.println(Message.getString(packet.getData()));
                    } else {
                        System.out.println("Responsed,but mime type error.");
                    }
                } else if (Message.ExecutType.UPLOAD == executType) {
                    if (cmds.length < 2) {
                        System.out.println("Upload application should be specify app folder.");
                        continue;
                    }
                    String folder = cmds[1];
                    File file = new File(folder);
                    if (!file.exists()) {
                        System.out.println("The specify app folder is not exist.");
                        continue;
                    }
                    String fileName = file.getName() + ".app";

                    // 1. Compress folder.
                    if (Zip.compress(folder, fileName)) {

                        // 2. Convert to byte[].
                        BufferedInputStream inputStream = new BufferedInputStream(new FileInputStream(fileName));
                        ByteArrayOutputStream out = new ByteArrayOutputStream(inputStream.available());
                        byte[] temp = new byte[inputStream.available()];
                        int size = 0;
                        while ((size = inputStream.read(temp)) != -1) {
                            out.write(temp, 0, size);
                        }
                        inputStream.close();
                        byte[] content = out.toByteArray();

                        // 3. Build packet.
                        InternetPacket packet = new InternetPacket();
                        packet.setExecutType(Message.ExecutType.UPLOAD.ordinal());
                        packet.setMimeType(Message.MIMEType.APP.ordinal());
                        byte[] appName = Message.getBytes(file.getName());
                        byte[] appNameSize = Convertor.int2byte(appName.length, 4);
                        byte[] data = new byte[4 + appName.length + content.length];
                        System.arraycopy(appNameSize, 0, data, 0, 4);
                        System.arraycopy(appName, 0, data, 4, appName.length);
                        System.arraycopy(content, 0, data, 4 + appName.length, content.length);
                        packet.setData(data);
                        File appFile = new File(fileName);
                        appFile.deleteOnExit();
                        packet = client.send(packet);
                        String responseInfo = Message.getString(packet.getData());
                        System.out.println(responseInfo);

                    } else {
                        System.out.println("The specify app folder compress error.");
                    }

                } else {
                    System.out.println("Unknow execute type:" + cmds[0]);
                }

            } catch (IOException e) {

            }
        }
    }
}
