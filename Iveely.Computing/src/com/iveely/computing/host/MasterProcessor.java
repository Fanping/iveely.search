package com.iveely.computing.host;

import com.iveely.computing.common.Message;
import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * Mster event processor.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:03:49
 */
public class MasterProcessor implements ICallback {

    /**
     * All slaves.
     */
    private final TreeMap<String, Integer> slaves;

    /**
     * Slaves sorted by performance.
     */
    private final List<String> performanceSlaves;

    /**
     * Event dispatcher.
     */
    private Dispatcher dispatcher;

    /**
     * Logger
     */
    private final Logger logger = Logger.getLogger(MasterProcessor.class.getName());

    public MasterProcessor() {
        slaves = new TreeMap<>();
        performanceSlaves = new ArrayList<>();
        dispatcher = new Dispatcher();
    }

    @Override
    public InternetPacket invoke(InternetPacket packet) {
        Message.ExecutType executType = Message.getExecuteType(packet.getExecutType());
        logger.info("Master recive:" + executType.name());

        // 1. Heartbeat.
        if (executType == Message.ExecutType.HEARTBEAT) {
            String[] infor = Message.getString(packet.getData()).split(":");
            if (infor.length == 3) {
                String address = infor[0] + ":" + infor[1];
                int runningAppCount = Integer.parseInt(infor[2]);
                slaves.put(address, runningAppCount);
                List arrayList = new ArrayList(slaves.entrySet());
                Collections.sort(arrayList, (Object o1, Object o2) -> {
                    Map.Entry obj1 = (Map.Entry) o1;
                    Map.Entry obj2 = (Map.Entry) o2;
                    return ((Integer) obj1.getValue()).compareTo((Integer) obj2
                            .getValue());
                });

                performanceSlaves.clear();
                for (Object arrayList1 : arrayList) {
                    String temp = ((Map.Entry) arrayList1).getKey().toString();
                    performanceSlaves.add(temp);
                }

                InternetPacket respPacket = new InternetPacket();
                respPacket.setExecutType(Message.ExecutType.RESPHEARTBEAT.ordinal());
                respPacket.setMimeType(Message.MIMEType.MESSAGE.ordinal());
                String[] keys = new String[slaves.size()];
                keys = slaves.keySet().toArray(keys);
                String allClients = String.join(" ", keys);
                respPacket.setData(Message.getBytes(allClients));
                return respPacket;

            }
        }

        // 2. Show all slaves.
        if (executType == Message.ExecutType.SLAVES) {
            Iterator iter = slaves.entrySet().iterator();
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

        // 3. Others.
        if (executType == Message.ExecutType.UPLOAD
                || executType == Message.ExecutType.RUN
                || executType == Message.ExecutType.LIST
                || executType == Message.ExecutType.KILLTASK) {
            return dispatcher.callSlaves(performanceSlaves, packet);
        }

        // 4. Unknown.
        return InternetPacket.getUnknowPacket();
    }
}
