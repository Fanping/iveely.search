/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.service;

import com.iveely.framework.text.json.JsonArray;
import com.iveely.framework.text.json.JsonObject;
import com.iveely.framework.text.json.JsonUtil;
import com.iveely.framework.text.json.JsonValue;
import java.io.File;
import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author 凡平
 */
public class Config {

    /**
     * Notify emails.
     */
    private List<String> notifies;

    /**
     * All plugins.
     */
    private List<PluginUnit> units;
    
    private Config() {
        notifies = new ArrayList<>();
        units = new ArrayList<>();
    }

    /**
     * Single instance.
     */
    private static Config config;
    
    public static Config getInstance() {
        if (config == null) {
            config = new Config();
        }
        return config;
    }
    
    public List<PluginUnit> getUnits() {
        return units;
    }
    
    public static void nofity() {
        if (MailSender.getInstance().IsValid() && !getInstance().notifies.isEmpty()) {
            String infor = "目前插件情况：\r\n";
            for (PluginUnit unit : getInstance().units) {
                infor += "=================";
                infor += unit.toDetail();
            }
            for (String email : getInstance().notifies) {
                MailSender.getInstance().sendEmail(email, "【插件情况信息】", infor);
            }
        }
    }
    
    public static void update() {
        // 1. Check file is exist.
        File file = new File("plugin.json");
        if (!file.exists()) {
            if (MailSender.getInstance().IsValid() && !getInstance().notifies.isEmpty()) {
                for (String notify : getInstance().notifies) {
                    MailSender.getInstance().sendEmail(notify, "严重错误", "找不到配置文件 plugin.json，请立刻配置！");
                }
            }
            return;
        }

        // 2. Get config information.
        String jsonValue = com.iveely.framework.file.Reader.readToString(file, "gb2312");
        String mail = JsonObject.readFrom(jsonValue).get("email").toString().replace("\"", "");
        MailSender.getInstance().setFrom(mail);
        
        String password = JsonObject.readFrom(jsonValue).get("password").toString().replace("\"", "");
        MailSender.getInstance().setPassword(password);
        
        String emailPort = JsonObject.readFrom(jsonValue).get("emailPort").toString().replace("\"", "");
        MailSender.getInstance().setPort(Integer.parseInt(emailPort));
        
        String emailName = JsonObject.readFrom(jsonValue).get("emailName").toString().replace("\"", "");
        MailSender.getInstance().setUser(emailName);
        
        String emailServer = JsonObject.readFrom(jsonValue).get("emailServer").toString().replace("\"", "");
        MailSender.getInstance().setServer(emailServer);
        
        String[] notifies = JsonObject.readFrom(jsonValue).get("notify").toString().replace("\"", "").split(",");
        getInstance().cleanNofify();
        for (String notify : notifies) {
            getInstance().appendNotify(notify);
        }

        // 3. Build plugins.
        getInstance().units.clear();
        JsonArray plugins = JsonObject.readFrom(jsonValue).get("plugins").asArray();
        for (JsonValue val : plugins) {
            jsonValue = val.toString();
            String name = JsonObject.readFrom(jsonValue).get("name").toString().replace("\"", "");
            String enable = JsonObject.readFrom(jsonValue).get("enable").toString().replace("\"", "");
            String pattern = JsonObject.readFrom(jsonValue).get("pattern").toString().replace("\"", "").replace("\\\\", "\\");
            String command = JsonObject.readFrom(jsonValue).get("command").toString().replace("\"", "");
            String accident = JsonObject.readFrom(jsonValue).get("owner").toString().replace("\"", "");
            Integer executeType = Integer.parseInt(JsonObject.readFrom(jsonValue).get("executeType").toString().replace("\"", ""));
            String[] ip = JsonObject.readFrom(jsonValue).get("ip").toString().replace("\"", "").split(",");
            String[] backup = JsonObject.readFrom(jsonValue).get("backup").toString().replace("\"", "").split(",");
            String[] exclusions = JsonObject.readFrom(jsonValue).get("exclusion").toString().replace("\"", "").split(",");
            PluginUnit unit = new PluginUnit();
            unit.setName(name);
            unit.setEnable(enable.equals("1"));
            unit.setNotifyEmail(accident);
            unit.setIpAddress(ip[0]);
            unit.setCommand(command);
            unit.setExecuteType(executeType);
            unit.setPort(Integer.parseInt(ip[1]));
            unit.setBakIpAddress(backup[0]);
            unit.setBakPort(Integer.parseInt(backup[1]));
            unit.setPattern(pattern);
            unit.addExclusion(exclusions);
            getInstance().units.add(unit);
            if (MailSender.getInstance().IsValid()) {
                MailSender.getInstance().sendEmail(accident, "【插件加载】" + name, unit.toSimple());
            }
        }
    }

    /**
     * Append notify email.
     *
     * @param email
     */
    public void appendNotify(String email) {
        notifies.add(email);
    }
    
    public void cleanNofify() {
        notifies.clear();
    }
}
