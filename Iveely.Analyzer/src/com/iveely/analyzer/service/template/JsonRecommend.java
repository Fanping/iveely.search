/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.analyzer.service.template;

import com.iveely.framework.text.json.JsonUtil;
import java.util.List;

/**
 *
 * @author X1 Carbon
 */
public class JsonRecommend {

    private String type;

    private String commond;

    private List<Object[]> data;

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
     * @return the data
     */
    public List<Object[]> getData() {
        return data;
    }

    /**
     * @param data the data to set
     */
    public void setData(List<Object[]> data) {
        this.data = data;
    }

    public String toJson() {
        return JsonUtil.beanToJson(this);
    }
}
