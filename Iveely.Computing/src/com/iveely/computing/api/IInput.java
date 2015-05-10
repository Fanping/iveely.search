package com.iveely.computing.api;

import com.iveely.computing.zookeeper.ZookeeperClient;
import java.util.HashMap;
import java.util.UUID;
import org.apache.log4j.Logger;

/**
 * Data Input.
 *
 * @author liufanping@iveely.com
 * @date 2015-3-4 19:43:01
 */
public abstract class IInput implements Cloneable {

    /**
     * Name of the data-input.
     */
    private String name = this.getClass().getName() + "(" + UUID.randomUUID().toString() + ")";

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(IInput.class.getName());

    /**
     * Initialize.
     *
     * @param conf
     * @param collector
     */
    public abstract void start(HashMap<String, Object> conf, StreamChannel collector);

    /**
     * Declare field to output.
     *
     * @param declarer
     */
    public abstract void declareOutputFields(FieldsDeclarer declarer);

    /**
     * Next data.
     */
    public abstract void nextTuple();

    /**
     * Data to which output.
     *
     */
    public abstract void toOutput();

    /**
     * Get name of the task.
     *
     * @return
     */
    public String getName() {
        return name;
    }

    /**
     * Prepare before execute.
     *
     * @param conf
     */
    public abstract void end(HashMap<String, Object> conf);

    /**
     * Set public cache. Everyone can access it.
     *
     * @param key
     * @param value
     */
    public void setPublicCache(String key, String value) {
        ZookeeperClient.getInstance().setNodeValue("/cache/" + key, value);
    }

    /**
     * Get public cache. Everyone can get it.
     *
     * @param key
     * @return
     */
    public String getPublicCache(String key) {
        String value = ZookeeperClient.getInstance().getNodeValue("/cache/" + key);
        return value;
    }

    /**
     * Clone self.
     *
     * @return
     */
    public IInput cloneSelf() {
        IInput o = null;
        try {
            o = (IInput) super.clone();
            o.name = this.getClass().getName() + "(" + UUID.randomUUID().toString() + ")";
        } catch (CloneNotSupportedException e) {
            logger.error(e);
        }
        return o;
    }
}
