package com.iveely.computing.host;

import com.iveely.computing.common.Message;
import com.iveely.computing.status.SystemConfig;
import com.iveely.framework.java.Convertor;
import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import java.io.DataOutputStream;
import java.io.FileOutputStream;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import org.apache.log4j.Logger;

/**
 * Mster event processor.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:03:49
 */
public class MasterProcessor implements ICallback {

    /**
     * Event dispatcher.
     */
    private final Dispatcher dispatcher;

    /**
     * Logger
     */
    private final Logger logger = Logger.getLogger(MasterProcessor.class.getName());

    public MasterProcessor() {
        dispatcher = new Dispatcher();
    }

    @Override
    public InternetPacket invoke(InternetPacket packet) {
        try {
            Message.ExecutType executType = Message.getExecuteType(packet.getExecutType());
            logger.info("Master recive:" + executType.name());

            // 1. Heartbeat.
            if (executType == Message.ExecutType.HEARTBEAT) {
                String[] infor = Message.getString(packet.getData()).split(":");
                if (infor.length == 3) {
                    String address = infor[0] + ":" + infor[1];
                    int runningAppCount = Integer.parseInt(infor[2]);
                    Luggage.slaves.put(address, runningAppCount);
                    List arrayList = new ArrayList(Luggage.slaves.entrySet());
                    Collections.sort(arrayList, (Object o1, Object o2) -> {
                        Map.Entry obj1 = (Map.Entry) o1;
                        Map.Entry obj2 = (Map.Entry) o2;
                        return ((Integer) obj1.getValue()).compareTo((Integer) obj2
                                .getValue());
                    });

                    Luggage.performanceSlaves.clear();
                    for (Object al : arrayList) {
                        String temp = ((Map.Entry) al).getKey().toString();
                        Luggage.performanceSlaves.add(temp);
                    }

                    InternetPacket respPacket = new InternetPacket();
                    respPacket.setExecutType(Message.ExecutType.RESPHEARTBEAT.ordinal());
                    respPacket.setMimeType(Message.MIMEType.MESSAGE.ordinal());
                    String[] keys = new String[Luggage.slaves.size()];
                    keys = Luggage.slaves.keySet().toArray(keys);
                    String allClients = String.join(" ", keys);
                    respPacket.setData(Message.getBytes(allClients));
                    return respPacket;

                }
            }

            // 2. Show all slaves.
            if (executType == Message.ExecutType.SLAVES) {
                Iterator iter = Luggage.slaves.entrySet().iterator();
                StringBuilder response = new StringBuilder();
                while (iter.hasNext()) {
                    Map.Entry entry = (Entry) iter.next();
                    String key = entry.getKey().toString();
                    String value = entry.getValue().toString();
                    response.append(key).append(":").append(value).append("\n");
                }
                if (response.length() < 1) {
                    response.append("Not Slaves.");
                }
                InternetPacket respPacket = new InternetPacket();
                respPacket.setExecutType(Message.ExecutType.RESPSLAVES.ordinal());
                respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
                respPacket.setData(Message.getBytes(response.toString()));
                return respPacket;
            }

            // 4. If upload.
            if (executType == Message.ExecutType.UPLOAD) {

                InternetPacket respPacket = new InternetPacket();
                respPacket.setExecutType(Message.ExecutType.RESPUPLOADAPP.ordinal());
                respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
                respPacket.setData(Message.getBytes("Success."));

                //  4.1. Extract application's name.
                byte[] data = packet.getData();
                byte[] lengthOfName = new byte[4];
                System.arraycopy(data, 0, lengthOfName, 0, 4);
                int nameSize = Convertor.bytesToInt(lengthOfName);
                byte[] nameBytes = new byte[nameSize];
                System.arraycopy(data, 4, nameBytes, 0, nameSize);
                String topology = Message.getString(nameBytes);

                // 4.2. Extract data.
                byte[] content = new byte[data.length - 4 - nameSize];
                System.arraycopy(data, nameSize + 4, content, 0, content.length);
                String uploadPath = SystemConfig.appFoler + "/" + topology + ".master.jar";
                try (DataOutputStream dbAppender = new DataOutputStream(new FileOutputStream(uploadPath, false))) {
                    dbAppender.write(content);
                    dbAppender.flush();

                } catch (Exception e) {
                    respPacket.setData(Message.getBytes(e.toString()));
                    logger.error(e);
                    return respPacket;
                }

                // 4.3. Run it.
                com.iveely.framework.java.JarExecutor executor;
                try {
                    executor = new com.iveely.framework.java.JarExecutor();
                    String paramValue[] = {uploadPath, topology};
                    Class paramClass[] = {String.class};
                    executor.invokeJarMain(uploadPath, topology, paramValue);
                } catch (Exception e) {
                    logger.error(e);
                    respPacket.setData(Message.getBytes(e.toString()));
                }
                executor = null;
                return respPacket;
            }

            // 3. Others.
            if (executType == Message.ExecutType.RUN
                    || executType == Message.ExecutType.LIST
                    || executType == Message.ExecutType.KILLTASK) {
                return dispatcher.callSlaves(packet);
            }
        } catch (Exception e) {
            logger.error(e);
        }

        // 4. Unknown.
        return InternetPacket.getUnknowPacket();
    }
}
