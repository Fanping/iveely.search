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
package com.iveely.computing.host;

import com.eclipsesource.json.JsonObject;
import com.iveely.computing.common.Message;
import com.iveely.computing.config.Paths;
import com.iveely.computing.config.SystemConfig;
import com.iveely.computing.coordinate.Coordinator;
import com.iveely.framework.compile.JarExecutor;
import com.iveely.framework.net.AsynServer;
import com.iveely.framework.net.Packet;
import com.iveely.framework.text.Convertor;

import org.apache.log4j.Logger;

import java.io.DataOutputStream;
import java.io.FileOutputStream;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

/**
 * Mster event processor.
 *
 * @author Iveely Liu
 */
public class MasterProcessor implements AsynServer.IHandler {

  /**
   * Event dispatcher.
   */
  private final Dispatcher dispatcher;

  /**
   * Node validator.
   */
  private final SignValidator validator;

  /**
   * Logger
   */
  private final Logger logger = Logger.getLogger(MasterProcessor.class.getName());

  /**
   * Build processor for master.
   *
   * @param vali Node validator.
   */
  public MasterProcessor(SignValidator vali) {
    dispatcher = new Dispatcher();
    validator = vali;
    validator.setMasterProcessor(this);
  }

  @Override
  public Packet process(Packet requesData) {
    Thread.currentThread().setName("master processor thread");
    Packet packet = requesData;
    Message.ExecuteType executeType = Message.getExecuteType(packet.getExecuteType());
    try {

      // 1. Heartbeat.
      if (executeType == Message.ExecuteType.HEARTBEAT) {
        String beatInfor = Message.getString((byte[])packet.getData());
        String ipaddress = JsonObject.readFrom(beatInfor).get("ipaddress").toString().replace("\"", "");
        String port = JsonObject.readFrom(beatInfor).get("port").toString().replace("\"", "");
        String usedSlotCount = JsonObject.readFrom(beatInfor).get("usedSlot").toString().replace("\"", "");
        String address = ipaddress + "," + port;
        logger.info("Master recive:" + executeType.name() + "," + address);
        int usedSlot = Integer.parseInt(usedSlotCount);
        Luggage.slaves.put(address, usedSlot);
        List arrayList = new ArrayList(Luggage.slaves.entrySet());
        Collections.sort(arrayList, (Object o1, Object o2) -> {
          Map.Entry obj1 = (Map.Entry) o1;
          Map.Entry obj2 = (Map.Entry) o2;
          return ((Integer) obj1.getValue()).compareTo((Integer) obj2.getValue());
        });

        Luggage.performanceSlaves.clear();
        for (Object al : arrayList) {
          String temp = ((Map.Entry) al).getKey().toString();
          Luggage.performanceSlaves.add(temp);
        }
        validator.arrive(address);
        Packet respPacket = new Packet();
        respPacket.setExecuteType(Message.ExecuteType.RESPHEARTBEAT.ordinal());
        respPacket.setMimeType(Message.MIMEType.MESSAGE.ordinal());
        String[] keys = new String[Luggage.slaves.size()];
        keys = Luggage.slaves.keySet().toArray(keys);
        String allClients = String.join(" ", keys);
        respPacket.setData(Message.getBytes(allClients));
        return respPacket;

      }

      // 2. Show all slaves.
      if (executeType == Message.ExecuteType.SLAVES) {
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
        Packet respPacket = new Packet();
        respPacket.setExecuteType(Message.ExecuteType.RESPSLAVES.ordinal());
        respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
        respPacket.setData(Message.getBytes(response.toString()));
        return respPacket;
      }

      // 4. If upload.
      if (executeType == Message.ExecuteType.UPLOAD) {

        Packet respPacket = new Packet();
        respPacket.setExecuteType(Message.ExecuteType.RESPUPLOADAPP.ordinal());
        respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
        respPacket.setData(Message.getBytes("Success."));

        // 4.1. Extract application's name.
        byte[] data = (byte[])packet.getData();
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
          logger.error("When process upload jar on master processor,exception happend.", e);
          return respPacket;
        }

        // 4.3. Run it.
        JarExecutor executor;
        try {
          executor = new JarExecutor();
          String paramValue[] = {uploadPath, topology};
          executor.invokeJarMain(uploadPath, topology, paramValue);
        } catch (Exception e) {
          logger.error("When execute jar on master processor,exception happend.", e);
          respPacket.setData(Message.getBytes(e.toString()));
        }
        return respPacket;
      }

      // 4. Rebalance topology.
      if (executeType == Message.ExecuteType.REBALANCE) {
        Packet respPacket = dispatcher.callSlaves(packet);
        JarExecutor executor;

        try {
          String name = Message.getString((byte[])packet.getData());
          Coordinator.getInstance().deleteNode(Paths.getTopologyRoot(name));
          String topology = Coordinator.getInstance().getNodeValue(Paths.getTopologyMapper(name));
          String uploadPath = SystemConfig.appFoler + "/" + topology + ".master.jar";
          executor = new JarExecutor();
          String paramValue[] = {uploadPath, topology};
          executor.invokeJarMain(uploadPath, topology, paramValue);
        } catch (Exception e) {
          logger.error("When rebalance on master processor,exception happend.", e);
          respPacket.setData(Message.getBytes(e.toString()));
        }
        return respPacket;
      }

      // 5. Others.
      if (executeType == Message.ExecuteType.RUN || executeType == Message.ExecuteType.LIST
          || executeType == Message.ExecuteType.KILLTASK) {
        return dispatcher.callSlaves(packet);
      }
    } catch (Exception e) {
      logger.error("Data parse error with execute type " + executeType, e.getCause());
    }

    // 4. Unknown.
    return Packet.getUnknownPacket();
  }

  @Override
  public void caught(String exception) {
    logger.error(exception);
  }
}
