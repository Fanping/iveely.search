package com.iveely.framework.net.cache;

import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.text.StringUtils;

/**
 * The call back of cache node when key is changed.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-13 20:01:24
 */
public class MemoryEvent implements ICallback {

    /**
     * Observer of memory.
     */
    private final MemoryObserver observer;

    public MemoryEvent(MemoryObserver observer) {
        this.observer = observer;
    }

    @Override
    public InternetPacket invoke(InternetPacket packet) {
        if (packet != null && packet.getExecutType() != 999) {
            String content = StringUtils.getString(packet.getData());
            String[] infor = content.split("#k-v#");
            //TODO: To do sth make sure the format is right.
            this.observer.onKeyEvent(infor[0], infor[1]);
        }
        //TODO:When return unknow-packet,make sure this is right.
        return InternetPacket.getUnknowPacket();
    }
}
