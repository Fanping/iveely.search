/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.framework.net;

/**
 * Message call back.
 *
 * @author liufanping@iveely.com
 */
public interface ICallback {

    /**
     * call back method.
     *
     * @param packet InternetPacket
     * @return
     */
    public InternetPacket invoke(InternetPacket packet);
}
