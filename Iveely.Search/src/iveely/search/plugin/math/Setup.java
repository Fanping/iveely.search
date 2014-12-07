package iveely.search.plugin.math;

import com.iveely.framework.net.Internet;
import com.iveely.framework.net.Server;
import com.iveely.framework.net.cache.Memory;

/**
 * Setup math as plugin.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 11:23:05
 */
public class Setup {

    /**
     * Server to listen the query.
     */
    private Server server;

    /**
     * Query's call back.
     */
    private CallBack callBack;

    public String invoke(String arg) {

        // 1. Init arguments.
        int port = Internet.getAvaliablePort(7800);
        callBack = new CallBack();
        server = new Server(callBack, port);
        Memory.getInstance().initCache("Common/allClients.txt", false);

        // 2. Add flag in memory.
        Memory.getInstance().set("calculator", Internet.getLocalIpAddress() + ":" + port);
        Memory.getInstance().set("calculator_expression", "\\s*[+-]?\\d+\\.?\\d*\\b\\s*");
        Memory.getInstance().append("plugin", "calculator");
        server.start();
        return "";
    }
}
