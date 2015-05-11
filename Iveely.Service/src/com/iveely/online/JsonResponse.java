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
public class JsonResponse {

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
    
    private String query;

    private String data;

    /**
     * @return the data
     */
    public String getData() {
        return data;
    }

    /**
     * @param data the data to set
     */
    public void setData(String data) {
        this.data = data;
    }

    public String toJson() {
        return JsonUtil.beanToJson(this);
    }

    /**
     * @return the query
     */
    public String getQuery() {
        return query;
    }

    /**
     * @param query the query to set
     */
    public void setQuery(String query) {
        this.query = query;
    }
    
    private String funName;

    /**
     * @return the funName
     */
    public String getFunName() {
        return funName;
    }

    /**
     * @param funName the funName to set
     */
    public void setFunName(String funName) {
        this.funName = funName;
    }
}
