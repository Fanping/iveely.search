package iveely.search.service;

import com.iveely.framework.net.websocket.EchoWebSocket;
import java.io.IOException;

/**
 * Master service (UI->Master)
 *
 * @author liufanping@iveely.com
 * @date 2014-11-1 18:25:16
 */
public class MasterService {

    /**
     * WebSocket provides external services.
     */
    private EchoWebSocket webSocket;

    /**
     * UI message callback.
     */
    private UCallback uiCallback;

    /**
     * Initialization.
     */
    private void init() {
        uiCallback = new UCallback();
        uiCallback.init();

    }

    public String invoke(String arg) {
        init();

        // 1. UI listen.
        try {
            webSocket = new EchoWebSocket(uiCallback, 9101);
            webSocket.service();
        } catch (IOException e) {
            return e.getMessage();
        }
        return "S_Failure";
    }

}
