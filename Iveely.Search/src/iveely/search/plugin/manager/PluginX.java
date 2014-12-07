package iveely.search.plugin.manager;

import com.iveely.framework.net.Client;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.text.StringUtils;
import java.util.regex.Pattern;

/**
 * The plugin entity.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 11:46:38
 */
public class PluginX {

    /**
     * The name of the plugin
     */
    private String name;

    /**
     * @return the name
     */
    public String getName() {
        return name;
    }

    /**
     * @param name the name to set
     */
    public void setName(String name) {
        this.name = name;
    }

    /**
     * The expression of the plugin
     */
    private String expression;

    /**
     * Pattern for match
     */
    private Pattern pattern;

    /**
     * @return the expression
     */
    public String getExpression() {
        return expression;
    }

    /**
     * @param expression the expression to set
     */
    public void setExpression(String expression) {
        this.expression = expression;
        try {
            pattern = Pattern.compile(expression);
        } catch (Exception e) {
            pattern = null;
        }
    }

    /**
     * The ip address of the plugin
     */
    private String ipAddress;

    /**
     * @return the ipAddress
     */
    public String getIpAddress() {
        return ipAddress;
    }

    /**
     * @param ipAddress the ipAddress to set
     */
    public void setIpAddress(String ipAddress) {
        this.ipAddress = ipAddress;
    }

    /**
     * The port to connect
     */
    private int port;

    /**
     * @return the port
     */
    public int getPort() {
        return port;
    }

    /**
     * @param port the port to set
     */
    public void setPort(int port) {
        this.port = port;
    }

    private boolean enable;

    /**
     * Is match user's query
     *
     * @param query
     * @return
     */
    public boolean isMatch(String query) {
        if (pattern == null) {
            return false;
        }
        return pattern.matcher(query).find();
    }

    /**
     * Get result by the query.
     *
     * @param query
     * @return
     */
    public String getResult(String query) {
        try {
            Client client = new Client(ipAddress, port);
            InternetPacket packet = new InternetPacket();
            packet.setData(StringUtils.getBytes(query));
            packet = client.send(packet);
            String result = StringUtils.getString(packet.getData());
            return result;
        } catch (Exception e) {
            return null;
        }
    }

    /**
     * @return the enable
     */
    public boolean isEnable() {
        return enable;
    }

    /**
     * @param enable the enable to set
     */
    public void setEnable(boolean enable) {
        this.enable = enable;
    }
}
