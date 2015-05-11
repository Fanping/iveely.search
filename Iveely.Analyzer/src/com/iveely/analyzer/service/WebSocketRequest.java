/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.service;

import com.iveely.framework.net.websocket.IEventProcessor;
import com.iveely.framework.text.json.JsonObject;
import com.iveely.analyzer.service.template.JsonError;
import java.util.Calendar;

/**
 *
 * @author X1 Carbon
 */
public class WebSocketRequest implements IEventProcessor {

    private final SCallback callback;

    public WebSocketRequest(SCallback callback) {
        this.callback = callback;
    }

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
                switch (cmd) {
                    case "search":
                        return callback.query(data);
                    case "suggest":
                        return callback.suggest(data);
                    case "recommend":
                        return callback.recommend(data);
                    case "detail":
                        return callback.detail(data);
                    case "image":
                        return callback.image(data);
                    default:
                        JsonError error = new JsonError();
                        error.setType("Error");
                        error.setCommond(cmd);
                        error.setDetail("command is not right.");
                        return error.toJson();
                }
            } else {
                JsonError error = new JsonError();
                error.setType("Error");
                error.setCommond(cmd);
                error.setDetail("Security code error.");
                return error.toJson();
            }
        } catch (Exception e) {
            JsonError error = new JsonError();
            error.setType("Error");
            error.setCommond("Unkown");
            error.setDetail(e.getMessage());
            return error.toJson();
        }
    }

}
