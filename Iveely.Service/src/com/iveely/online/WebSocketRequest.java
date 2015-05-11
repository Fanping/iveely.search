/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.online;

import com.iveely.framework.net.Client;
import com.iveely.framework.net.websocket.IEventProcessor;
import com.iveely.framework.text.json.JsonObject;
import com.iveely.framework.text.json.JsonUtil;
import com.iveely.service.Config;
import com.iveely.service.PluginUnit;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;

/**
 *
 * @author X1 Carbon
 */
public class WebSocketRequest implements IEventProcessor {

    @Override
    public String invoke(String data) {
        try {
            String cmd = JsonObject.readFrom(data).get("command").toString().replace("\"", "");
            String security = JsonObject.readFrom(data).get("security").toString().replace("\"", "");
            String verifyCode = Calendar.getInstance().get(Calendar.DAY_OF_YEAR) + ""
                    + Calendar.getInstance().get(Calendar.DAY_OF_MONTH) + ""
                    + Calendar.getInstance().get(Calendar.DAY_OF_WEEK) + ""
                    + Calendar.getInstance().get(Calendar.HOUR_OF_DAY);
            if (security.equals(verifyCode)) {
                List<JsonResponse> list = new ArrayList<>();
                String query = JsonObject.readFrom(data).get("query").toString().replace("\"", "");
                List<PluginUnit> units = Config.getInstance().getUnits();
                List<Integer> readyGetUnits = new ArrayList<>();
                for (PluginUnit unit : units) {
                    if (unit.isEnable() && unit.isMatch(cmd, query)) {
                        if (!unit.hasExclusion(readyGetUnits)) {
                            String result = new Message().getResult(unit, query);
                            if (!result.isEmpty()) {
                                JsonResponse response = new JsonResponse();
                                response.setFunName(unit.getName());
                                response.setCommand(unit.getCommand());
                                response.setData(result);
                                response.setQuery(query);
                                list.add(response);
                                readyGetUnits.add(unit.getExecuteType());
                            }
                        }
                    }
                }
                JsonInfor infor = new JsonInfor();
                infor.setType("OK");
                infor.setCommond(cmd);
                infor.setDetail(JsonUtil.listToJson(list).replace("\"{", "{").replace("}\"", "}").replace("\"[", "[").replace("]\"", "]").replace("\\\"", "\""));
                return infor.toJson().replace("\"{", "{").replace("}\"", "}").replace("\"[", "[").replace("]\"", "]").replace("\\\"", "\"");
            } else {
                JsonInfor error = new JsonInfor();
                error.setType("Error");
                error.setCommond(cmd);
                error.setDetail("Security code error.");
                return error.toJson();
            }
        } catch (Exception e) {
            JsonInfor error = new JsonInfor();
            error.setType("Error");
            error.setCommond("Unkown");
            error.setDetail(e.toString());
            return error.toJson();
        }
    }
}
