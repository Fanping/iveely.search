/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.online;

import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import com.iveely.service.MailSender;
import com.iveely.service.PluginUnit;
import java.io.UnsupportedEncodingException;

/**
 *
 * @author 凡平
 */
public class Message {

    public String getResult(PluginUnit unit, String query) {
        try {
            Client client = new Client(unit.getIpAddress(), unit.getPort());
            InternetPacket packet = new InternetPacket();
            packet.setExecutType(unit.getExecuteType());
            packet.setMimeType(0);
            packet.setData(getBytes(query));
            InternetPacket respPacket = client.send(packet);
            if (respPacket.getExecutType() == unit.getExecuteType() * -1) {
                String result = getString(respPacket.getData());
                return result;
            }
        } catch (Exception e) {
            if (MailSender.getInstance().IsValid()) {
                MailSender.getInstance().sendEmail(unit.getNotifyEmail(), "【服务运行错误】:" + unit.getName() + " 【query】:" + query, e.getMessage());
            }
        }

        try {
            Client client = new Client(unit.getBakIpAddress(), unit.getBakPort());
            InternetPacket packet = new InternetPacket();
            packet.setExecutType(unit.getExecuteType());
            packet.setMimeType(0);
            packet.setData(getBytes(query));
            InternetPacket respPacket = client.send(packet);
            if (respPacket.getExecutType() == unit.getExecuteType() * -1) {
                String result = getString(respPacket.getData());
                return result;
            }
        } catch (Exception e) {
            if (MailSender.getInstance().IsValid()) {
                MailSender.getInstance().sendEmail(unit.getNotifyEmail(), "【服务(备份)运行错误】:" + unit.getName() + " 【query】:" + query, e.getMessage());
            }
        }

        return "";
    }

    /**
     * Convert string to byte[].
     *
     * @param content
     * @return
     */
    private byte[] getBytes(String content) {
        byte[] bytes;
        try {
            bytes = content.getBytes("UTF-8");
        } catch (UnsupportedEncodingException ex) {
            bytes = content.getBytes();
        }
        return bytes;
    }

    /**
     * Convert byte[] to string.
     *
     * @param bytes
     * @return
     */
    private String getString(byte[] bytes) {
        try {
            return new String(bytes, "UTF-8").trim();
        } catch (UnsupportedEncodingException ex) {
            return new String(bytes).trim();
        }
    }
}
