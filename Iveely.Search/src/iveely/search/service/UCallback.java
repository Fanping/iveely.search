package iveely.search.service;

import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.net.cache.Memory;
import com.iveely.framework.net.websocket.IEventProcessor;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.TreeSet;
import org.apache.log4j.Logger;

/**
 * UI message callback.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-1 21:34:54
 */
public class UCallback implements IEventProcessor {

    /**
     * Text search client.
     */
    private List<String> textClients;

    /**
     * Image search client.
     */
    private List<String> imageClients;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(UCallback.class.getName());

    /**
     * Initialization.
     */
    public void init() {
        textClients = new ArrayList<>();
        imageClients = new ArrayList<>();
        //TODO:need registe to distribute memory cache, when search clients have changed.
        Memory.getInstance().initCache("Common/allClients.txt", false);
    }

    @Override
    public String invoke(String data) {
        if (data.startsWith("[SEARCH]:")) {
            // [SEARCH]:大学
            data = data.replace("[SEARCH]:", "");
            data = notifyClients(textClients, data, Exchange.ExecuteType.SEARCH);
            String[] records = data.split("\\[WEIGHT\\]:");
            TreeSet<SpeciObj> list = new TreeSet<>();
            for (String recod : records) {
                String[] infor = recod.split("\\[PAGEID\\]:");
                if (infor.length == 2) {
                    SpeciObj speciObj = new SpeciObj();
                    speciObj.setRank(Double.parseDouble(infor[0]));
                    speciObj.setObj("[PAGEID]:" + infor[1]);
                    list.add(speciObj);
                }
            }
            String result = "";
            Iterator<SpeciObj> it = list.iterator();
            while (it.hasNext()) {
                SpeciObj obj = it.next();
                result += obj.getObj().toString().replace("[PAGERECORD]", "[WEIGHT]:" + obj.getRank() + "\n[PAGERECORD]\n");
            }
            return "1_" + result;

        } else if (data.startsWith("[IMAGE]:")) {
            // [SEARCH]:大学
            data = data.replace("[IMAGE]:", "");
            data = notifyClients(imageClients, data, Exchange.ExecuteType.IMAGE);
            String[] records = data.split("\\[WEIGHT\\]:");
            TreeSet<SpeciObj> list = new TreeSet<>();
            for (String recod : records) {
                String[] infor = recod.split("\\[IMAGEID\\]:");
                if (infor.length == 2) {
                    SpeciObj speciObj = new SpeciObj();
                    speciObj.setRank(Double.parseDouble(infor[0]));
                    speciObj.setObj("[IMAGEID]:" + infor[1]);
                    list.add(speciObj);
                }
            }
            String result = "";
            Iterator<SpeciObj> it = list.iterator();
            while (it.hasNext()) {
                SpeciObj obj = it.next();
                result += obj.getObj().toString().replace("[IMAGERECORD]", "[WEIGHT]:" + obj.getRank() + "\n[IMAGERECORD]\n");
            }
            return "2_" + result;

        } else if (data.startsWith("[SCORE]:")) {
            // [SCORE]:127.0.0.1_Port_Keyword-PageId
            data = data.replace("[SCORE]:", "");
            String[] infor = data.split("_");
            if (infor.length == 5) {
                return notifySpecifyClient(infor[0], Integer.parseInt(infor[1]), Exchange.ExecuteType.ADDSCORE, infor[2] + "-" + infor[3] + "-" + infor[4]);
            } else {
                return "[SEARCH]:" + "Unknow";
            }
        } else if (data.startsWith("[SNAPSHOT]:")) {
            // [SNAPSHOT]:127.0.0.1_Port_Keyword-PageId
            data = data.replace("[SNAPSHOT]:", "");
            String[] infor = data.split("_");
            if (infor.length == 4) {
                return notifySpecifyClient(infor[0], Integer.parseInt(infor[1]), Exchange.ExecuteType.SNAPSHOT, infor[2] + "-" + infor[3]);
            } else {
                return "[SNAPSHOT]:" + "Unknow";
            }
        } else {
            return "Unknow";
        }
    }

    /**
     * Notify all search client.
     *
     * @param data
     * @return
     */
    private String notifyClients(List<String> clientList, String data, Exchange.ExecuteType type) {
        StringBuilder response = new StringBuilder();
        logger.info("client count:" + clientList.size());
        boolean isSend = false;
        for (String str : clientList) {
            logger.info("send to :" + str);
            String[] infor = str.split(":");
            if (infor.length == 2) {
                isSend = true;
                response.append(notifySpecifyClient(infor[0], Integer.parseInt(infor[1]), type, data));
            }
        }
        if (!isSend) {
            if (type == Exchange.ExecuteType.IMAGE) {
                loadImageClient();
            }
            if (type == Exchange.ExecuteType.SEARCH) {
                loadTextClient();
            }
        }
        return response.toString();
    }

    /**
     * Notify the designated search client.
     *
     * @param data
     * @return
     */
    private String notifySpecifyClient(String ip, int port, Exchange.ExecuteType type, String data) {
        Client client = new Client(ip, port);
        InternetPacket packet = new InternetPacket();
        packet.setExecutType(type.ordinal());
        packet.setData(Exchange.getBytes(data));
        InternetPacket respPacket = client.send(packet);
        if (respPacket.getExecutType() != 999) {
            return Exchange.getString(respPacket.getData());
        } else {
            return "NULL";
        }
    }

    /**
     * Load clients.
     */
    private void loadClient() {
        loadImageClient();
        loadTextClient();
    }

    /**
     * Load text search clients.
     */
    private void loadTextClient() {
        //textClients.add("127.0.0.1:6500");
         getClientsFromMemeory("[SLAVESERVICE_TEXT]", textClients);
    }

    /**
     * Load image search clients.
     */
    private void loadImageClient() {
        getClientsFromMemeory("[SLAVESERVICE_IMAGE]", imageClients);
    }

    /**
     * Get clients information from memory cache.
     *
     * @param flag
     * @param clients
     */
    private void getClientsFromMemeory(String flag, List<String> clients) {
        String[] allClients = Memory.getInstance().get(flag).split("#RECORD#");
        clients.clear();
        for (String clientStr : allClients) {
            if (!clients.contains(clientStr)) {
                clients.add(clientStr);
            }
        }
    }
}
