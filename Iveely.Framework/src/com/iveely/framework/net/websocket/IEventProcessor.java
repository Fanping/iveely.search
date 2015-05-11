/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.framework.net.websocket;

/**
 * Event processor of websocket.
 *
 * @author liufanping@iveely.com
 */
public interface IEventProcessor {

    /**
     * 回调函数
     *
     * @param data
     * @return
     */
    public String invoke(String data);
}
