package com.iveely.computing.config;

/**
 * Master deploy information.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:25:49
 */
class MasterDeploy {

    /**
     * Service port.
     */
    private int port;

    /**
     * Get port.
     *
     * @return Get port.
     */
    public int getPort() {
        return port;
    }

    /**
     * Set port.
     *
     * @param port
     */
    public void setPort(int port) {
        this.port = port;
    }

    /**
     * Ip address.
     */
    private String hostAddress;

    /**
     * Get host.
     *
     * @return host address.
     */
    public String getHostAddress() {
        return hostAddress;
    }

    /**
     * Set host address.
     *
     * @param hostAddress Ip address.
     */
    public void setHostAddress(String hostAddress) {
        this.hostAddress = hostAddress;
    }

    /**
     * Get default config.
     *
     * @return
     */
    public static MasterDeploy GetDefault() {
        MasterDeploy masterDeploy = new MasterDeploy();
        masterDeploy.setPort(8001);
        masterDeploy.setHostAddress("127.0.0.1");
        return masterDeploy;
    }
}
