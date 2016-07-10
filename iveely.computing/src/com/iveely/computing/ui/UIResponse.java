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
package com.iveely.computing.ui;

import com.eclipsesource.json.JsonObject;
import com.iveely.computing.common.Message;
import com.iveely.framework.net.AsynServer;
import com.iveely.framework.net.Packet;
import com.iveely.framework.net.websocket.SocketServer.IHandler;
import com.iveely.framework.net.websocket.WSHandler;
import com.iveely.framework.text.JSONUtil;

import org.apache.log4j.Logger;

/**
 * UI response,all data is JSON format.
 */
public class UIResponse implements IHandler {

  /**
   * Master event.
   */
  private final AsynServer.IHandler masterEvent;

  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(UIResponse.class.getName());

  /**
   * Password of UI response.
   */
  private final String password;

  /**
   * Build ui response.
   *
   * @param masterEvent Master event.
   * @param uiPwd       UI password.
   */
  public UIResponse(AsynServer.IHandler masterEvent, String uiPwd) {
    this.masterEvent = masterEvent;
    this.password = uiPwd;
  }

  @Override
  public String invoke(Integer sessionId, WSHandler handler, String data) {
    try {
      String query = JsonObject.readFrom(data).get("command").toString().replace("\"", "");

      // 1. Show cluster summary
      if (query.equals("cluster summary")) {
        return showClusterSummary();
      }

      // 2. Show topology summary
      if (query.equals("topology summary")) {
        return showTopologySummary();
      }

      // 3. Show slave summary
      if (query.equals("slave summary")) {
        return showSlaveSummary();
      }

      // 4. Show system summary
      if (query.equals("system summary")) {
        return showSystemSummary();
      }

      // 5. Query topology.
      if (query.equals("query topology")) {
        String name = JsonObject.readFrom(data).get("topology").toString().replace("\"", "");
        return queryTopology(name);
      }

      // 5. Task summary.
      if (query.equals("task summary")) {
        String name = JsonObject.readFrom(data).get("topology").toString().replace("\"", "");
        return taskSummary(name);
      }

      // 6. Statistic summary.
      if (query.equals("statistic summary")) {
        String name = JsonObject.readFrom(data).get("topology").toString().replace("\"", "");
        return statisticSummary(name);
      }

      // 7. Kill topology
      if (query.equals("kill")) {
        String name = JsonObject.readFrom(data).get("topology").toString().replace("\"", "");
        String pwd = JsonObject.readFrom(data).get("password").toString().replace("\"", "");
        return killTopology(name, pwd);
      }

      // 8. Rebalance topology
      if (query.equals("rebalance")) {
        String name = JsonObject.readFrom(data).get("topology").toString().replace("\"", "");
        String pwd = JsonObject.readFrom(data).get("password").toString().replace("\"", "");
        return rebalanceTopology(name, pwd);
      }

    } catch (Exception e) {
      logger.error(e);
    }
    return "";
  }

  /**
   * Show cluster information.
   */
  private String showClusterSummary() {
    ClusterSummary summary = new ClusterSummary();
    summary.get();
    String data = JSONUtil.toString(summary);
    return data;
  }

  /**
   * Show topology summary.
   */
  private String showTopologySummary() {
    TopologySummary summary = new TopologySummary();
    summary.init();
    String data = summary.toJson();
    return data;
  }

  /**
   * Show slave summary.
   */
  private String showSlaveSummary() {
    SlaveSummary summary = new SlaveSummary();
    summary.get();
    String data = summary.toJson();
    return data;
  }

  /**
   * Show system summary.
   */
  private String showSystemSummary() {
    SystemSummary summary = new SystemSummary();
    summary.get();
    String data = summary.toJson();
    return data;
  }

  /**
   * Query topology name.
   */
  private String queryTopology(String name) {
    TopologySummary summary = new TopologySummary();
    String data = summary.queryToJson(name).replace("\"[", "[").replace("]\"", "]");
    return data;
  }

  /**
   * Show task summary.
   */
  private String taskSummary(String tpName) {
    TaskSummary summary = new TaskSummary();
    summary.get(tpName);
    String data = summary.toJson().replace("\"[", "[").replace("]\"", "]");
    return data;
  }

  /**
   * Show statistic summary.
   */
  private String statisticSummary(String tpName) {
    StatisticSummary summary = new StatisticSummary();
    summary.get(tpName);
    String data = summary.toJson().replace("\"[", "[").replace("]\"", "]");
    return data;
  }

  /**
   * Kill topology.
   */
  private String killTopology(String name, String pwd) {
    if (pwd.equals(this.password)) {
      Packet packet = new Packet();
      packet.setExecuteType(Message.ExecuteType.KILLTASK.ordinal());
      packet.setData(Message.getBytes(name));
      packet.setMimeType(Message.MIMEType.MESSAGE.ordinal());
      Packet resp = (Packet) this.masterEvent.process(packet);
      String data = Message.getString((byte[]) resp.getData());
      return "{\"resType\":\"kill topology\",\"respData\":\"" + data.replace("\n", "") + "\"}";
    } else {
      return "{\"resType\":\"kill topology\",\"respData\":\"Password error.\"}";
    }
  }

  /**
   * Rebalance topology.
   */
  private String rebalanceTopology(String name, String pwd) {
    if (pwd.equals(this.password)) {
      Packet packet = new Packet();
      packet.setExecuteType(Message.ExecuteType.REBALANCE.ordinal());
      packet.setData(Message.getBytes(name));
      packet.setMimeType(Message.MIMEType.MESSAGE.ordinal());
      Packet resp = (Packet) this.masterEvent.process(packet);
      String data = Message.getString((byte[]) resp.getData());
      return "{\"resType\":\"rebalance topology\",\"respData\":\"" + data + "\"}";
    } else {
      return "{\"resType\":\"rebalance topology\",\"respData\":\"Password error.\"}";
    }
  }

  @Override
  public void close(Integer sessionId) {

  }
}
