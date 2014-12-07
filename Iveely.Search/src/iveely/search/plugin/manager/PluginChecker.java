package iveely.search.plugin.manager;

import com.iveely.framework.net.cache.Memory;
import java.util.Map;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * The updater of the plugin.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 10:45:26
 */
public class PluginChecker implements Runnable {

    /**
     * All plugins on search system
     */
    private final TreeMap<String, PluginX> plugins;

    /**
     * Logger(log4j)
     */
    private final Logger logger = Logger.getLogger(PluginChecker.class.getName());

    public PluginChecker() {
        plugins = new TreeMap<>();
    }

    @Override
    public void run() {
        Memory.getInstance().initCache("Common/allClients.txt", false);
        Memory.getInstance().register("plugin");
        while (true) {
            try {
                String records = Memory.getInstance().get("plugin");
                logger.info("check plugin service:" + records);
                String[] pluginInfors = records.split("#RECORD#");
                for (String infor : pluginInfors) {
                    if (!plugins.containsKey(infor)) {
                        String ipAddress = Memory.getInstance().get(infor);
                        if (ipAddress != null) {
                            String[] ipPort = ipAddress.split(":");
                            String expression = Memory.getInstance().get(infor + "_expression");
                            if (expression.length() > 0 && ipPort.length == 2) {
                                PluginX pluginX = new PluginX();
                                pluginX.setName(infor);
                                pluginX.setExpression(expression);
                                pluginX.setPort(Integer.parseInt(ipPort[1]));
                                pluginX.setIpAddress(ipPort[0]);
                                pluginX.setEnable(true);
                                plugins.put(infor, pluginX);
                            }
                        }
                    }
                }
                Thread.sleep(1000 * 60);
            } catch (InterruptedException | NumberFormatException e) {
            }
        }
    }

    /**
     * Get result by plugin
     *
     * @param query
     * @return
     */
    public String getResult(String query) {
        logger.info("get plugin query:" + query);
        for (Map.Entry entiy : plugins.entrySet()) {
            PluginX pluginX = (PluginX) entiy.getValue();
            if (pluginX.isEnable() && pluginX.isMatch(query)) {
                String result = pluginX.getResult(query);
                if (result == null) {
                    // Most happened here is plugin not in service, so disable it.
                    pluginX.setEnable(false);
                    logger.info("plugin manager disable plugin named:" + pluginX.getName());
                    return "";
                }
                logger.info("plugin manager get result by:" + pluginX.getName());
                return result;
            }
        }
        return "";
    }
}
