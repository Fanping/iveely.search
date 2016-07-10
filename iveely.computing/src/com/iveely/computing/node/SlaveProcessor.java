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
package com.iveely.computing.node;

import com.iveely.computing.api.TopologySubmitter;
import com.iveely.computing.common.Message;
import com.iveely.computing.config.SystemConfig;
import com.iveely.framework.compile.JarExecutor;
import com.iveely.framework.net.AsynServer;
import com.iveely.framework.net.Packet;
import com.iveely.framework.text.Convertor;

import org.apache.log4j.Logger;

import java.io.DataOutputStream;
import java.io.FileOutputStream;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;

/**
 * Event processor on slave.
 */
public class SlaveProcessor implements AsynServer.IHandler {

  /**
   * The observer of distribute cache.
   */
  private final TreeMap<String, List<String>> observers;

  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(SlaveProcessor.class.getName());

  /**
   * Build slave processor.
   */
  public SlaveProcessor() {
    this.observers = new TreeMap<>();
  }

  /**
   * Process upload application.
   */
  private Packet processUploadApp(Packet packet) {

    // 1. Extract application's name.
    byte[] data = (byte[])packet.getData();
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
      dbAppender.flush();
      logger.info(String.format("2[Upload]. Upload to slave as store path %s success.", uploadPath));
    } catch (Exception e) {
      logger.info(String.format("2[Upload]. Upload to slave as store path %s failure due to %s", uploadPath, e.toString()));
      responseInfo += e.toString();
      Packet respPacket = new Packet();
      respPacket.setData(Message.getBytes(responseInfo));
      respPacket.setExecuteType(Message.ExecuteType.RESPUPLOADAPP.ordinal());
      respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
      return respPacket;
    }

    // 3. Throw to thread pool and run it.
    JarExecutor executor;
    logger.info(String.format("3[Upload]. Prepare to execute jar, local path is %s,arguments:%s,%s", uploadPath, runArgs[0], runArgs[1]));
    try {
      executor = new JarExecutor();
      String paramValue[] = {runArgs[1]};
      executor.invokeJarMain(uploadPath, runArgs[0], paramValue);
      logger.info(String.format("4[Upload]. Finish run jar with success."));
    } catch (Exception e) {
      responseInfo += e.getMessage();
      logger.info(String.format("4[Upload]. Finish run jar with error,%s", e.toString()));
    }

    // 4. Response packet.
    Packet respPacket = new Packet();
    respPacket.setData(Message.getBytes(responseInfo));
    respPacket.setExecuteType(Message.ExecuteType.RESPUPLOADAPP.ordinal());
    respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
    return respPacket;
  }

  /**
   * Build response packet.
   */
  private Packet buildResponse(String responseData, Message.ExecuteType responseExecutType) {
    Packet respPacket = new Packet();
    respPacket.setData(Message.getBytes(responseData));
    respPacket.setExecuteType(responseExecutType.ordinal());
    respPacket.setMimeType(Message.MIMEType.TEXT.ordinal());
    return respPacket;
  }

  @Override
  public Packet process(Packet request) {
    Thread.currentThread().setName("Slave processor thread");
    Packet packet = (Packet) request;
    Message.ExecuteType executeType = Message.getExecuteType(packet.getExecuteType());

    // 1. Process upload.
    if (executeType == Message.ExecuteType.UPLOAD) {
      logger.info(String.format("1[Upload]. Get command from master,execute type is %s", executeType.toString()));
      return processUploadApp(packet);
    }

    // // 3. Process add new key-val.
    // if (executType == Message.ExecuteType.SETCACHE) {
    // String[] infor = Message.getString(packet.getData()).split("#k-v#");
    // String key = infor[0];
    // String val = infor[1];
    // cache.add(key, val);
    // if (observers.containsKey(key)) {
    // //TODO:Send to observers, if observer is not exist,please remove it.
    // List<String> observerAddress = observers.get(key);
    // observerAddress.stream().forEach((String observer) -> {
    // try {
    // String[] obsIpAndPort = observer.split(":");
    // Client client = new Client(obsIpAndPort[0],
    // Integer.parseInt(obsIpAndPort[1]));
    // InternetPacket callbackPacket = new InternetPacket();
    // callbackPacket.setExecutType(Message.ExecuteType.CALLBACKOFKEYEVENT.ordinal());
    // callbackPacket.setMimeType(Message.MIMEType.MESSAGE.ordinal());
    // callbackPacket.setData(Message.getBytes(key + "#k-v#" + val));
    // client.send(packet);
    // } catch (NumberFormatException e) {
    // observerAddress.remove(observer);
    // }
    // });
    // }
    // return buildResponse("key is set success.",
    // Message.ExecuteType.RESPSETCACHE);
    // }
    //
    // // 4. Process get the value.
    // if (executType == Message.ExecuteType.GETCACHE) {
    // String key = Message.getString(packet.getData());
    // Object val = cache.read(key);
    // String resp = "";
    // if (val != null) {
    // resp = val.toString();
    // }
    // return buildResponse(resp, Message.ExecuteType.RESPGETCACHE);
    // }
    //
    // // 5. Prcess append value to specify key.
    // if (executType == Message.ExecuteType.APPENDCACHE) {
    // String[] infor = Message.getString(packet.getData()).split("#k-v#");
    // String key = infor[0];
    // String val = infor[1];
    // Object objVal = cache.read(key);
    // if (objVal != null) {
    // val = objVal + "#RECORD#" + val;
    // }
    // cache.add(key, val);
    // if (observers.containsKey(key)) {
    // //TODO:Send to observers, if observer is not exist,please remove it.
    // List<String> observerAddress = observers.get(key);
    // for (String observer : observerAddress) {
    // try {
    // String[] obsIpAndPort = observer.split(":");
    // Client client = new Client(obsIpAndPort[0],
    // Integer.parseInt(obsIpAndPort[1]));
    // InternetPacket callbackPacket = new InternetPacket();
    // callbackPacket.setExecutType(Message.ExecuteType.CALLBACKOFKEYEVENT.ordinal());
    // callbackPacket.setMimeType(Message.MIMEType.MESSAGE.ordinal());
    // callbackPacket.setData(Message.getBytes(key + "#k-v#" + val));
    // client.send(packet);
    // } catch (NumberFormatException e) {
    // observerAddress.remove(observer);
    // }
    //
    // }
    // }
    // return buildResponse("key is append success.",
    // Message.ExecuteType.RESPSETCACHE);
    // }
    // 2. Process register memory service.
    if (executeType == Message.ExecuteType.REGISTE) {
      String[] infor = Message.getString((byte[])packet.getData()).split("_");
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
      return buildResponse("Regist success.", Message.ExecuteType.RESPREGIST);
    }

    // 8. Process kill task.
    if (executeType == Message.ExecuteType.KILLTASK) {
      String tpName = Message.getString((byte[])packet.getData());
      String respStr = TopologySubmitter.stop(tpName);
      return buildResponse(respStr, Message.ExecuteType.RESPKILLTASK);
    }

    // 9. Check is online.
    if (executeType == Message.ExecuteType.ISONLINE) {
      return buildResponse("On Line", Message.ExecuteType.RESPISONLINE);
    }

    // 10. Rebalance task.
    if (executeType == Message.ExecuteType.REBALANCE) {
      String tpName = Message.getString((byte[])packet.getData());
      String respStr = TopologySubmitter.stop(tpName);
      return buildResponse(respStr, Message.ExecuteType.RESPREBALANCE);
    }

    return Packet.getUnknownPacket();
  }

  @Override
  public void caught(String exception) {
    throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
  }
}
