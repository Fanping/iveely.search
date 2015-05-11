/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.online;

import com.iveely.framework.text.json.JsonUtil;

/**
 *
 * @author 凡平
 */
public class JsonInfor {

    private String type;

    private String commond;

    private String detail;

    /**
     * @return the type
     */
    public String getType() {
        return type;
    }

    /**
     * @param type the type to set
     */
    public void setType(String type) {
        this.type = type;
    }

    /**
     * @return the commond
     */
    public String getCommond() {
        return commond;
    }

    /**
     * @param commond the commond to set
     */
    public void setCommond(String commond) {
        this.commond = commond;
    }

    /**
     * @return the detail
     */
    public String getDetail() {
        return detail;
    }

    /**
     * @param detail the detail to set
     */
    public void setDetail(String detail) {
        this.detail = detail;
    }

    public static String getDefault() {
        JsonInfor error = new JsonInfor();
        error.setType("Error");
        error.setCommond("Unkown");
        error.setDetail("Unkown");
        return error.toJson();
    }

    public String toJson() {
        return JsonUtil.beanToJson(this);
    }
}
