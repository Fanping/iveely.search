package com.iveely.framework.text;

import java.net.URI;
import java.net.URISyntaxException;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

/**
 * Url misc.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-21 21:58:09
 */
public class UrlMisc {

    /**
     * Get URL's hostname.
     *
     * @param url
     * @return
     */
    public static String getHost(String url) {
        try {
            URI uri = new URI(url);
            return uri.getAuthority();
        } catch (URISyntaxException ex) {
        }
        return null;
    }

    /**
     * Get URL's domain.
     *
     * @param url
     * @return
     */
    public static String getDomain(String url) {
        String host = getHost(url);
        if (host != null) {
            String[] domains = host.split("\\.");
            if (domains.length > 2 && !isIPAddress(host)) {
                int index = 1;
                StringBuilder domainsString = new StringBuilder();
                for (int i = index; i < domains.length - 1; i++) {
                    domainsString.append(domains[i]).append(".");
                }
                domainsString.append(domains[domains.length - 1]);
                return domainsString.toString();
            } else {
                return host;
            }
        }
        return null;
    }

    /**
     * Whether is the IP address.
     *
     * @param ip
     * @return
     */
    public static boolean isIPAddress(String ip) {
        Pattern p = Pattern.compile("([0-9]|[0-9]\\d|1\\d{2}|2[0-1]\\d|25[0-5])(\\.(\\d|[0-9]\\d|1\\d{2}|2[0-4]\\d|25[0-5])){3}");
        Matcher m = p.matcher(ip);
        boolean match = m.matches();
        return match;
    }

    /**
     * Is the url domain.
     *
     * @param url
     * @return
     */
    public static boolean isDomainUrl(String url) {
        url = url.toLowerCase();
        String domain = getDomain(url);
        String host = getHost(url);
        if (host == null || domain == null) {
            return false;
        }
        if ((domain == null ? host == null : domain.equals(host)) || (("www." + domain) == null ? host == null : ("www." + domain).equals(host))) {
            if (url.endsWith(domain) || url.endsWith(domain + "/")) {
                return true;
            }
        }
        return false;
    }

    /**
     * Is the host name.
     *
     * @param url
     * @return
     */
    public static boolean isHostUrl(String url) {
        String host = getHost(url);
        if (host == null) {
            return false;
        }
        return url.endsWith(host);
    }
}
