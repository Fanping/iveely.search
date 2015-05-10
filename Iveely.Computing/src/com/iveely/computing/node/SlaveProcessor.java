package com.iveely.computing.node;

import com.iveely.computing.api.TopologySubmitter;
import com.iveely.computing.common.Message;
import com.iveely.computing.status.SystemConfig;
import com.iveely.framework.java.Convertor;
import com.iveely.framework.net.Client;
import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.text.CyclingBuffer;
import java.io.DataOutputStream;
import java.io.FileOutputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * Event process on slave.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 15:39:55
 */
public class SlaveProcessor implements ICallback {

    /**
     * Distribute memory cache.
     */
    private final CyclingBuffer cache;

    /**
     * The observer of distribute cache.
     */
    private final TreeMap<String, List<String>> observers;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(SlaveProcessor.class.getName());
    
    public SlaveProcessor() {
        cache = new CyclingBuffer(10000 * 1000);
        //TODO:If the cache was covered by other key. please change the observer.
        observers = new TreeMap<>();
    }
    
    @Override
    public InternetPacket invoke(InternetPacket packet) {
        Message.ExecutType executType = Message.getExecuteType(packet.getExecutType());
        logger.info(executType.toString());

        // 1. Process upload.
        if (executType == Message.ExecutType.UPLOAD) {
            return processUploadApp(packet);
        }

        // 2. Process run.
        if (executType == Message.ExecutType.RUN) {
            String[] infor = Message.getString(packet.getData()).split(":");
            String appName = infor[0];
            String dependencyApp = infor[1];
            String flag = infor[2];
            String respStr = Attribute.getInstance().runApp(appName, dependencyApp, flag);
            return buildResponse(respStr, Message.ExecutType.RESPRUNAPP);
        }

        // 3. Process add new key-val.
        if (executType == Message.ExecutType.SETCACHE) {
            String[] infor = Message.getString(packet.getData()).split("#k-v#");
            String key = infor[0];
            String val = infor[1];
            cache.add(key, val);
            if (observers.containsKey(key)) {
                //TODO:Send to observers, if observer is not exist,please remove it.
                List<String> observerAddress = observers.get(key);
                observerAddress.stream().forEach((String observer) -> {
                    try {
                        String[] obsIpAndPort = observer.split(":");
                        Client client = new Client(obsIpAndPort[0], Integer.parseInt(obsIpAndPort[1]));
                        InternetPacket callbackPacket = new InternetPacket();
                        callbackPacket.setExecutType(Message.ExecutType.CALLBACKOFKEYEVENT.ordinal());
                        callbackPacket.setMimeType(Message.MIMEType.MESSAGE.ordinal());
                        callbackPacket.setData(Message.getBytes(key + "#k-v#" + val));
                        client.send(packet);
                    } catch (NumberFormatException e) {
                        observerAddress.remove(observer);
                    }
                });
            }
            return buildResponse("key is set success.", Message.ExecutType.RESPSETCACHE);
        }

        //  4. Process get the value.
        if (executType == Message.ExecutType.GETCACHE) {
            String key = Message.getString(packet.getData());
            Object val = cache.read(key);
            String resp = "";
            if (val != null) {
                resp = val.toString();
            }
            return buildResponse(resp, Message.ExecutType.RESPGETCACHE);
        }

        // 5. Prcess append value to specify key.
        if (executType == Message.ExecutType.APPENDCACHE) {
            String[] infor = Message.getString(packet.getData()).split("#k-v#");
            String key = infor[0];
            String val = infor[1];
            Object objVal = cache.read(key);
            if (objVal != null) {
                val = objVal + "#RECORD#" + val;
            }
            cache.add(key, val);
            if (observers.containsKey(key)) {
                //TODO:Send to observers, if observer is not exist,please remove it.
                List<String> observerAddress = observers.get(key);
                for (String observer : observerAddress) {
                    try {
                        String[] obsIpAndPort = observer.split(":");
                        Client client = new Client(obsIpAndPort[0], Integer.parseInt(obsIpAndPort[1]));
                        InternetPacket callbackPacket = new InternetPacket();
                        callbackPacket.setExecutType(Message.ExecutType.CALLBACKOFKEYEVENT.ordinal());
                        callbackPacket.setMimeType(Message.MIMEType.MESSAGE.ordinal());
                        callbackPacket.setData(Message.getBytes(key + "#k-v#" + val));
                        client.send(packet);
                    } catch (NumberFormatException e) {
                        observerAddress.remove(observer);
                    }
                    
                }
            }
            return buildResponse("key is append success.", Message.ExecutType.RESPSETCACHE);
        }

        // 6. Process get all tasks.
        if (executType == Message.ExecutType.LIST) {
            String respStr = Attribute.getInstance().getAllApp();
            return buildResponse(respStr, Message.ExecutType.RESPLISTTASK);
        }

        // 7. Process register memory service.
        if (executType == Message.ExecutType.REGISTE) {
            String[] infor = Message.getString(packet.getData()).split("_");
            String key = infor[0];
            String observerAddress = infor[1];
            if (observers.containsKey(key)) {
                List<String> list = observers.get(key);
                list.add(observerAddress);
            } else {
                List<String> list = new ArrayList<>();
                list.add(observerAddress);
                observers.put(key, list);
            }
            return buildResponse("Regist success.", Message.ExecutType.RESPREGIST);
        }

        // 8. Process kill task.
        if (executType == Message.ExecutType.KILLTASK) {
            String[] infor = Message.getString(packet.getData()).split(" ");
            String tpName = Message.getString(packet.getData());
            String respStr = TopologySubmitter.stop(tpName);
            return buildResponse(respStr, Message.ExecutType.RESPKILLTASK);
        }
        return InternetPacket.getUnknowPacket();
    }

    /**
     * Process upload application.
     *
     * @return
     */
    private InternetPacket processUploadApp(InternetPacket packet) {

        // 1. Extract application's name.
        byte[] data = packet.getData();
        byte[] lengthOfName = new byte[4];
        System.arraycopy(data, 0, lengthOfName, 0, 4);
        int argSize = Convertor.bytesToInt(lengthOfName);
        byte[] argBytes = new byte[argSize];
        System.arraycopy(data, 4, argBytes, 0, argSize);
        String arguments = Message.getString(argBytes);
        String[] runArgs = arguments.split(":");
        String responseInfo = "";

        // 2. Extract data.
        byte[] content = new byte[data.length - 4 - argSize];
        System.arraycopy(data, argSize + 4, content, 0, content.length);
        String uploadPath = SystemConfig.appFoler + "/" + runArgs[0] + ".slave.jar";
        try (DataOutputStream dbAppender = new DataOutputStream(new FileOutputStream(uploadPath, false))) {
            dbAppender.write(content);
        } catch (Exception e) {
            responseInfo += e.toString();
            InternetPacket respPacket = new InternetPacket();
            respPacket.setData(Message.getBytes(responseInfo));
            respPacket.setExecutType(Message.ExecutType.RESPUPLOADAPP.ordinal());
            respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
            return respPacket;
        }

        // 3. Throw to thread pool and run it.
        com.iveely.framework.java.JarExecutor executor;
        try {
            executor = new com.iveely.framework.java.JarExecutor();
            String paramValue[] = {runArgs[1]};
            Class paramClass[] = {String.class};
            executor.invokeJarMain(uploadPath, runArgs[0], paramValue);
        } catch (Exception e) {
            responseInfo += e.getMessage();
        }

        // 4. Response packet.
        InternetPacket respPacket = new InternetPacket();
        respPacket.setData(Message.getBytes(responseInfo));
        respPacket.setExecutType(Message.ExecutType.RESPUPLOADAPP.ordinal());
        respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
        return respPacket;
    }

    /**
     * Build response packet.
     *
     * @param responseDate
     * @param responseExecutType
     * @return
     */
    private InternetPacket buildResponse(String responseDate, Message.ExecutType responseExecutType) {
        InternetPacket respPacket = new InternetPacket();
        respPacket.setData(Message.getBytes(responseDate));
        respPacket.setExecutType(responseExecutType.ordinal());
        respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
        return respPacket;
    }
}
