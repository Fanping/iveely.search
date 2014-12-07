package iveely.search.plugin.manager;

import com.iveely.framework.net.websocket.IEventProcessor;

/**
 * Event processor of plugin service.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 11:02:43
 */
public class EventProcessor implements IEventProcessor {

    /**
     * Checker for plugins
     */
    private final PluginChecker checker;

    public EventProcessor(PluginChecker checker) {
        this.checker = checker;
    }

    @Override
    public String invoke(String data) {
        return this.checker.getResult(data);
    }

}
