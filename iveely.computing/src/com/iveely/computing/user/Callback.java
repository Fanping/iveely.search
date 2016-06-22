/*
 * Copyright 2016 liufanping@iveely.com.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.iveely.computing.user;

import com.iveely.computing.common.Message;
import com.iveely.framework.net.AsynClient;
import com.iveely.framework.net.Packet;
import org.apache.log4j.Logger;

/**
 *
 * @author Administrator
 */
class Callback implements AsynClient.IHandler {

    private final Logger logger = Logger.getLogger(Callback.class);

    @Override
    public void receive(Packet response) {
        Packet packet = (Packet) response;
        Message.ExecuteType executeType = Message.getExecuteType(packet.getExecuteType());
        if (executeType == Message.ExecuteType.RESPLISTTASK) {
            logger.info(Message.getString(packet.getData()));
        } else {
            logger.error("Responsed,but execute type is error.");
        }
//        Message.MIMEType respMimeType = Message.getMIMEType(packet.getMimeType());
//        if (respMimeType == Message.MIMEType.MESSAGE) {
//            logger.info("Execute success.");
//        } else if (respMimeType == Message.MIMEType.TEXT) {
//            logger.info(Message.getString(packet.getData()));
//        }else if(respMimeType==Message.ExecuteType.) 
//        else {
//            logger.error("Responsed,but mime type error.");
//        }
    }

    @Override
    public void caught(String exception) {
        throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
    }

}
