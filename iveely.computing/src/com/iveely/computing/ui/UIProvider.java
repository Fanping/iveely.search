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
package com.iveely.computing.ui;

import com.iveely.computing.config.ConfigWrapper;
import com.iveely.framework.net.AsynServer;
import com.iveely.framework.net.websocket.SocketServer;

import org.apache.log4j.Logger;

/**
 * The UI to show cluster information.
 */
public class UIProvider implements Runnable {

    /**
     * Websocket server.
     */
    private SocketServer socket;

    /**
     * Response callback.
     */
    private UIResponse response;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(UIProvider.class.getName());

    /**
     * Build UI provider.
     *
     * @param masterEvent Master event.
     * @param uiPwd The UI password.
     */
    public UIProvider(AsynServer.IHandler masterEvent, String uiPwd) {
        try {
            this.response = new UIResponse(masterEvent, uiPwd);
            this.socket = new SocketServer(this.response, ConfigWrapper.get().getMaster().getUi_port());
        } catch (Exception e) {
            logger.error("When initialize ui provider,exception happend.", e);
        }

    }

    /**
     * Start ui.
     */
    @Override
    public void run() {
        Thread.currentThread().setName("ui provider thread");
        logger.info("UI service is starting...");
        this.socket.start();
    }
}
