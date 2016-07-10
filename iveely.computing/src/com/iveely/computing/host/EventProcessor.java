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
package com.iveely.computing.host;

import com.iveely.framework.net.websocket.SocketServer;
import com.iveely.framework.net.websocket.WSHandler;

/**
 * Event processor for web to master.
 *
 * @author Iveely Liu
 */
public class EventProcessor implements SocketServer.IHandler {

  @Override
  public String invoke(Integer sessionId, WSHandler handler, String data) {
    return data;
  }

  @Override
  public void close(Integer sessionId) {

  }

}
