package com.iveely.computing.user;

import com.iveely.computing.common.Message;
import com.iveely.computing.status.SystemConfig;
import com.iveely.computing.zookeeper.ZookeeperClient;
import com.iveely.framework.java.Convertor;
import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStreamReader;
import java.nio.charset.Charset;
import org.apache.log4j.Logger;

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

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Console.class.getName());

    public Console(String zkServer, int zkPort) throws Exception {
        SystemConfig.zkServer = zkServer;
        SystemConfig.zkPort = zkPort;
        getMasterInfor();
        client = new Client(SystemConfig.masterServer, SystemConfig.masterPort);
    }

    @Override
    public void run() {
        while (true) {
            System.out.print("Command:");
            try {
                InputStreamReader reader = new InputStreamReader(System.in, Charset.defaultCharset());
                BufferedReader inputReader = new BufferedReader(reader);
                String input = inputReader.readLine();
                String[] cmds = input.split(" ");
                processCmd(cmds);
            } catch (IOException e) {
                logger.error(e);
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
                logger.info("Execute success.");
            } else if (respMimeType == Message.MIMEType.TEXT) {
                logger.info(Message.getString(packet.getData()));
            } else {
                logger.error("Responsed,but mime type error.");
            }
        } else if (Message.ExecutType.UPLOAD == executType) {
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

            ByteArrayOutputStream out;
            try ( // 2. Convert to byte[].
                    BufferedInputStream inputStream = new BufferedInputStream(new FileInputStream(fileName))) {
                out = new ByteArrayOutputStream(inputStream.available());
                byte[] temp = new byte[inputStream.available()];
                int size = 0;
                while ((size = inputStream.read(temp)) != -1) {
                    out.write(temp, 0, size);
                }
            }
            byte[] content = out.toByteArray();

            // 3. Build packet.
            InternetPacket packet = new InternetPacket();
            packet.setExecutType(Message.ExecutType.UPLOAD.ordinal());
            packet.setMimeType(Message.MIMEType.APP.ordinal());
            byte[] appName = Message.getBytes(cmds[2]);
            byte[] appNameSize = Convertor.int2byte(appName.length, 4);
            byte[] data = new byte[4 + appName.length + content.length];
            System.arraycopy(appNameSize, 0, data, 0, 4);
            System.arraycopy(appName, 0, data, 4, appName.length);
            System.arraycopy(content, 0, data, 4 + appName.length, content.length);
            packet.setData(data);
            //File appFile = new File(fileName);
            //appFile.deleteOnExit();
            packet = client.send(packet);
            String responseInfo = Message.getString(packet.getData());
            System.out.println(responseInfo);

        } else {
            logger.error("Unknow execute type:" + cmds[0]);
        }
    }

    /**
     * Get master information.
     */
    private void getMasterInfor() throws Exception {
        String connectPath = ZookeeperClient.getInstance().getNodeValue(SystemConfig.masterRoot);
        if (connectPath == null || connectPath.isEmpty()) {
            throw new Exception("When get master information, connection string can not null or empty.");
        }
        String[] infor = connectPath.split(",");
        SystemConfig.masterServer = infor[0];
    }
}
