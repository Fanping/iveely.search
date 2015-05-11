/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.service;

import java.util.HashSet;
import java.util.List;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 *
 * @author 凡平
 */
public class PluginUnit {

    /**
     * Plugin name.
     */
    private String name;

    /**
     * @return the name
     */
    public String getName() {
        return name;
    }

    /**
     * @param name the name to set
     */
    public void setName(String name) {
        this.name = name;
    }

    /**
     * Whether enable.
     */
    private boolean enable;

    /**
     * @return the enable
     */
    public boolean isEnable() {
        return enable;
    }

    /**
     * @param enable the enable to set
     */
    public void setEnable(boolean enable) {
        this.enable = enable;
    }

    /**
     * Connect ip.
     */
    private String ipAddress;

    /**
     * @return the ipAddress
     */
    public String getIpAddress() {
        return ipAddress;
    }

    /**
     * @param ipAddress the ipAddress to set
     */
    public void setIpAddress(String ipAddress) {
        this.ipAddress = ipAddress;
    }

    /**
     * Backup ip address.
     */
    private String bakIpAddress;

    /**
     * @return the bakIpAddress
     */
    public String getBakIpAddress() {
        return bakIpAddress;
    }

    /**
     * @param bakIpAddress the bakIpAddress to set
     */
    public void setBakIpAddress(String bakIpAddress) {
        this.bakIpAddress = bakIpAddress;
    }

    /**
     * Plugin service port.
     */
    private int port;

    /**
     * @return the port
     */
    public int getPort() {
        return port;
    }

    /**
     * @param port the port to set
     */
    public void setPort(int port) {
        this.port = port;
    }

    /**
     * Plugin service backup port.
     */
    private int bakPort;

    /**
     * @return the bakPort
     */
    public int getBakPort() {
        return bakPort;
    }

    /**
     * @param bakPort the bakPort to set
     */
    public void setBakPort(int bakPort) {
        this.bakPort = bakPort;
    }

    /**
     * Accident sent mailboxes.
     */
    private String notifyEmail;

    /**
     * @return the notifyEmail
     */
    public String getNotifyEmail() {
        return notifyEmail;
    }

    /**
     * @param notifyEmail the notifyEmail to set
     */
    public void setNotifyEmail(String notifyEmail) {
        this.notifyEmail = notifyEmail;
    }

    private int count;

    /**
     * @return the count
     */
    public int getCount() {
        return count;
    }

    /**
     * @param count the count to set
     */
    public void setCount(int count) {
        this.count = count;
    }

    private String command;

    /**
     * @return the command
     */
    public String getCommand() {
        return command;
    }

    /**
     * @param command the command to set
     */
    public void setCommand(String command) {
        this.command = command;
    }

    private Pattern pattern;

    /**
     * @return the pattern
     */
    public String getPattern() {
        return pattern.pattern();
    }

    /**
     * @param patternVal
     */
    public void setPattern(String patternVal) {
        try {
            pattern = Pattern.compile(patternVal);
        } catch (Exception e) {
            if (MailSender.getInstance().IsValid()) {
                MailSender.getInstance().sendEmail(notifyEmail, "【规则错误】:" + pattern, e.getMessage());
                setEnable(false);
            }
        }

    }

    private HashSet<Integer> exclusions;

    public void addExclusion(String[] id) {
        if (exclusions == null) {
            exclusions = new HashSet<>();
        }
        exclusions.clear();
        for (String v : id) {
            if (!v.isEmpty()) {
                Integer iv = Integer.parseInt(v);
                if (!exclusions.contains(iv)) {
                    exclusions.add(iv);
                }
            }
        }
    }

    public boolean hasExclusion(List<Integer> id) {
        if (exclusions == null || exclusions.isEmpty()) {
            return false;
        }
        for (Integer v : id) {
            if (exclusions.contains(v)) {
                return true;
            }
        }
        return false;
    }

    public boolean isMatch(String command, String query) {
        if (this.command.equals(command)) {
            return true;
        }
        if (pattern != null && pattern.matcher(query).matches()) {
            return true;
        }
        return false;
    }

    private int executeType;

    public String toDetail() {
        return "插件名称:" + name + " \r\n配置状态：" + enable + " \r\n匹配模式：" + pattern + " \r\n消息命令：" + command + " \r\n服务器地址:" + ipAddress + " \r\n服务器端口:" + port + " \r\n备份服务器:" + bakIpAddress + " \r\n备份服务器端口:" + bakPort + " \r\n负责人:" + notifyEmail + " \r\n累积时段使用次数：" + count;
    }

    public String toSimple() {
        return "插件名称:" + name + " \r\n配置状态：" + enable + " \r\n匹配模式：" + pattern + " \r\n消息命令：" + command + " \r\n服务器地址:" + ipAddress + " \r\n服务器端口:" + port + " \r\n备份服务器:" + bakIpAddress + " \r\n备份服务器端口:" + bakPort;
    }

    /**
     * @return the executeType
     */
    public int getExecuteType() {
        return executeType;
    }

    /**
     * @param executeType the executeType to set
     */
    public void setExecuteType(int executeType) {
        this.executeType = executeType;
    }
}
