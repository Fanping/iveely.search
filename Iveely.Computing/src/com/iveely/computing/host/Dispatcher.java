package com.iveely.computing.host;

import com.iveely.computing.common.Message;
import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import java.util.Iterator;
import java.util.List;

/**
 * Dispatcher for master to slave.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 14:13:44
 */
public class Dispatcher {

    public Dispatcher() {

    }

    /**
     * Send message to slaves.
     *
     * @param slaves
     * @param packet
     * @return
     */
    public InternetPacket callSlaves(List<String> slaves, InternetPacket packet) {
        Message.ExecutType executType = Message.getExecuteType(packet.getExecutType());

        // 1. Upload application
        if (executType == Message.ExecutType.UPLOAD) {
            return processTask(slaves, packet, Message.ExecutType.RESPUPLOADAPP);
        }

        // 2. Execute application.
        if (executType == Message.ExecutType.RUN) {
            return processRunApp(slaves, packet);
        }

        // 3. Show all tasks.
        if (executType == Message.ExecutType.LIST) {
            return processTask(slaves, packet, Message.ExecutType.RESPLISTTASK);
        }

        // 4. Kill task.
        if (executType == Message.ExecutType.KILLTASK) {
            return processTask(slaves, packet, Message.ExecutType.RESPKILLTASK);
        }

        return InternetPacket.getUnknowPacket();
    }

    /**
     * Process run application task.
     *
     * @param slaves
     * @param packet
     * @return
     */
    private InternetPacket processRunApp(List<String> slaves, InternetPacket packet) {

        // 1. Prepare packet.
        InternetPacket respPacket = new InternetPacket();
        respPacket.setExecutType(Message.ExecutType.RESPRUNAPP.ordinal());
        respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());

        // 2. Process.
        StringBuilder responseText = new StringBuilder();
        String[] allcate = Message.getString(packet.getData()).split(" ");

        // 2.1 Is specify slaves count to run.
        String innerData = allcate[0];
        int sum = slaves.size();
        if (allcate.length == 2) {
            sum = Integer.parseInt(allcate[1]);
        }

        // 2.2 Is specify slaves by dependency.
        String dependencyAppString = "";
        if (allcate.length == 3 && "on".equals(allcate[1].toLowerCase())) {
            dependencyAppString = allcate[2];
        }

        // 2.3 Is specify slave by address.
        String specifyAddress = "";
        if (allcate.length == 3 && "at".equals(allcate[1].toLowerCase())) {
            if (slaves.contains(allcate[2])) {
                specifyAddress = allcate[2];
            } else {
                respPacket.setData(Message.getBytes("Can not find your slave."));
                return respPacket;
            }
        }

        // 2.4 Call slaves to run.
        Iterator salve = slaves.iterator();
        int currentId = 0;
        int count = sum;
        do {
            String[] info;
            if (specifyAddress.equals("")) {
                info = salve.next().toString().split(":");
            } else {
                info = specifyAddress.split(":");
            }
            String ip = info[0];
            int port = Integer.parseInt(info[1]);
            Client client = new Client(ip, port);

            // Add flag that 0-2 or 1-2,means has two slaves would be run. first number is id. 
            packet.setData(Message.getBytes(innerData + ":" + dependencyAppString + ":" + currentId + "-" + sum));
            InternetPacket slaveRespPacket = client.send(packet);
            responseText.append(info[0]).append(",").append(info[1]).append(":").append(Message.getString(slaveRespPacket.getData())).append("\n");
            count--;
            currentId++;
        } while (salve.hasNext() && count > 0 && specifyAddress.equals(""));

        // 3. Finish packet.
        respPacket.setData(Message.getBytes(responseText.toString()));
        return respPacket;
    }

    /**
     * Process other task.
     *
     * @param slaves
     * @param packet
     * @param respExecutType
     * @return
     */
    private InternetPacket processTask(List<String> slaves, InternetPacket packet, Message.ExecutType respExecutType) {

        // 1. Prepare packet.
        InternetPacket respPacket = new InternetPacket();
        respPacket.setExecutType(respExecutType.ordinal());
        respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
        StringBuilder responseText = new StringBuilder();

        // 2. Send to slaves.
        Iterator salve = slaves.iterator();
        while (salve.hasNext()) {
            String[] info = salve.next().toString().split(":");
            if (info.length == 2) {
                String ip = info[0];
                int port = Integer.parseInt(info[1]);
                Client client = new Client(ip, port);
                InternetPacket slaveRespPacket = client.send(packet);
                responseText.append(info[0]).append(",").append(info[1]).append(":").append(Message.getString(slaveRespPacket.getData())).append("\n");
            }
        }

        // 3. Finish packet.
        respPacket.setData(Message.getBytes(responseText.toString()));
        return respPacket;
    }
}
