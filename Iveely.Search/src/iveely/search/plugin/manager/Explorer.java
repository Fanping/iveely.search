package iveely.search.plugin.manager;

import com.iveely.framework.net.websocket.EchoWebSocket;
import java.io.IOException;

/**
 * The manager of the plugin system.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-15 16:55:46
 */
public class Explorer {

    /**
     * The plugin service for UI.
     */
    private EchoWebSocket pluginService;

    /**
     * The processor of any query.
     */
    private EventProcessor processor;

    /**
     * The updater for memory.
     */
    private PluginChecker pluginChecker;

    private boolean isInitSuccess;

    public Explorer() {
        try {
            pluginChecker = new PluginChecker();
            processor = new EventProcessor(pluginChecker);
            pluginService = new EchoWebSocket(processor, 9102);
            isInitSuccess = true;
        } catch (IOException e) {
            isInitSuccess = false;
        }

    }

    /**
     * Start service.
     *
     * @param arg
     * @return
     */
    public String invoke(String arg) {
        if (!isInitSuccess) {
            return "Not init success.";
        }
        // 1. PluginChecker run.
        Thread updateThread = new Thread(pluginChecker);
        updateThread.start();

        // 2. Service run to get query from user.
        pluginService.service();
        return "";
    }
}
