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

import com.iveely.computing.common.Message;
import com.iveely.framework.net.AsynClient;
import com.iveely.framework.net.Packet;

import org.apache.log4j.Logger;

import java.util.Iterator;
import java.util.Locale;

/**
 * Dispatcher for master to slave,responsible for task distribution.
 */
public class Dispatcher {

  /**
   * Logger
   */
  private final Logger logger = Logger.getLogger(Dispatcher.class.getName());

  /**
   * Build instance.
   */
  public Dispatcher() {

  }

  /**
   * Send message to slaves.
   */
  public Packet callSlaves(Packet packet) {
    Message.ExecuteType executeType = Message.getExecuteType(packet.getExecuteType());
    logger.info("callSlaves:" + executeType.name());

    // 1. Upload application
    if (executeType == Message.ExecuteType.UPLOAD) {
      return processTask(packet, Message.ExecuteType.RESPUPLOADAPP);
    }

    // 2. Execute application.
    if (executeType == Message.ExecuteType.RUN) {
      return processRunApp(packet);
    }

    // 3. Show all tasks.
    if (executeType == Message.ExecuteType.LIST) {
      return processTask(packet, Message.ExecuteType.RESPLISTTASK);
    }

    // 4. Kill task.
    if (executeType == Message.ExecuteType.KILLTASK) {
      return processTask(packet, Message.ExecuteType.RESPKILLTASK);
    }

    // 5. Rebalance task.
    if (executeType == Message.ExecuteType.REBALANCE) {
      return processTask(packet, Message.ExecuteType.RESPREBALANCE);
    }

    return Packet.getUnknownPacket();
  }

  /**
   * Process run application task.
   */
  private Packet processRunApp(Packet packet) {

    // 1. Prepare packet.
    Packet respPacket = new Packet();
    respPacket.setExecuteType(Message.ExecuteType.RESPRUNAPP.ordinal());
    respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());

    // 2. Process.
    StringBuilder responseText = new StringBuilder();
    String[] allcate = Message.getString((byte[])packet.getData()).split(" ");

    // 2.1 Is specify slaves count to run.
    String innerData = allcate[0];
    int sum = Luggage.performanceSlaves.size();
    if (allcate.length == 2) {
      sum = Integer.parseInt(allcate[1]);
    }

    // 2.2 Is specify slaves by dependency.
    String dependencyAppString = "";
    if (allcate.length == 3 && "on".equals(allcate[1].toLowerCase(Locale.CHINESE))) {
      dependencyAppString = allcate[2];
    }

    // 2.3 Is specify slave by address.
    String specifyAddress = "";
    if (allcate.length == 3 && "at".equals(allcate[1].toLowerCase(Locale.CHINESE))) {
      if (Luggage.performanceSlaves.contains(allcate[2])) {
        specifyAddress = allcate[2];
      } else {
        respPacket.setData(Message.getBytes("Can not find your slave."));
        return respPacket;
      }
    }

    // 2.4 Call slaves to run.
    Iterator salve = Luggage.performanceSlaves.iterator();
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
      AsynClient client = new AsynClient(ip, port, null);

      // Add flag that 0-2 or 1-2,means has two slaves would be run. first
      // number is id.
      packet.setData(Message.getBytes(innerData + ":" + dependencyAppString + ":" + currentId + "-" + sum));
      client.send(packet);
//            responseText.append(info[0]).append(",").append(info[1]).append(":")
//                    .append(Message.getString(slaveRespPacket.getData())).append("\n");
      count--;
      currentId++;
    } while (salve.hasNext() && count > 0 && specifyAddress.equals(""));

    // 3. Finish packet.
    respPacket.setData(Message.getBytes(responseText.toString()));
    return respPacket;
  }

  /**
   * Process other task.
   */
  private Packet processTask(Packet packet, Message.ExecuteType respExecutType) {

    // 1. Prepare packet.
    Packet respPacket = new Packet();
    respPacket.setExecuteType(respExecutType.ordinal());
    respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
    StringBuilder responseText = new StringBuilder();

    // 2. Send to slaves.
    Iterator salve = Luggage.performanceSlaves.iterator();
    while (salve.hasNext()) {
      String abs = salve.next().toString();
      logger.info("call slave begin->" + abs);
      String[] info = abs.split(":");
      if (info.length == 2) {
        String ip = info[0];
        int port = Integer.parseInt(info[1]);
        AsynClient client = new AsynClient(ip, port, null);
        client.send(packet);
//                responseText.append(info[0]).append(",").append(info[1]).append(":")
//                        .append(Message.getString(slaveRespPacket.getData())).append("\n");
      }
      logger.info("call slave end->" + abs);
    }

    // 3. Finish packet.
    respPacket.setData(Message.getBytes(responseText.toString()));
    return respPacket;
  }
}
