package com.iveely.computing.host;

import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.net.websocket.IEventProcessor;

/**
 * Event processor for web to master.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 17:06:17
 */
public class EventProcessor implements IEventProcessor {

    @Override
    public String invoke(String data) {
        return data;
    }

}
