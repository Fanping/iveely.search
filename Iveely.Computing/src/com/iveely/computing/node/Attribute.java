package com.iveely.computing.node;

import com.iveely.computing.app.IApplication;
import com.iveely.framework.file.Reader;
import java.io.File;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Collection;
import java.util.Date;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * Slave's attribute.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 15:00:27
 */
public class Attribute {

    /**
     * All applications on this slave.
     */
    private final TreeMap<String, App> apps;

    /**
     * Data folder.
     */
    private String folder;

    /**
     * Instance.
     */
    private static Attribute attribute;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Attribute.class.getName());

    private Attribute() {
        apps = new TreeMap<>();
    }

    /**
     * Get single instance.
     *
     * @return
     */
    public static Attribute getInstance() {
        if (attribute == null) {
            attribute = new Attribute();
        }
        return attribute;
    }

    /**
     * Get data folder.
     *
     * @return the folder
     */
    public String getFolder() {
        return folder;
    }

    /**
     * Set data folder.
     *
     * @param folder the folder to set
     */
    public void setFolder(String folder) {
        this.folder = folder;
        File file = new File(folder);
        if (!file.exists()) {
            if (!file.mkdir()) {
                logger.error(folder + " not created.");
            }
        }
//        setAppUploadFolder(this.folder + "/upload/");
//        setAppInstallFolder(this.folder + "/install/");
    }

    /**
     * Is contains this application.
     *
     * @param name
     * @return
     */
    public boolean isContainsApp(String name) {
        return apps.containsKey(name);
    }

    /**
     * add new application.
     *
     * @param file
     * @return
     */
    public boolean addApp(File file) {
        if (isContainsApp(file.getName())) {
            return false;
        }
        String[] configFiles = file.list();
        App app = new App();
        app.setAppName(file.getName());
        app.setUploadTime(new Date(file.lastModified()).toString());
        app.setSize(file.length());
        app.setStatus(IApplication.Status.JUSTINSTALL);
        String jarPath = "";
        String exeParams = "";
        String exeMethod = "invoke";
        String exeClass = "";
        IApplication.ExecutePlan exeCycle = IApplication.ExecutePlan.UNKOWN;
        for (String configFile : configFiles) {
            if (configFile.contains("app.run")) {
                List<String> settings = Reader.readAllLine(getAppInstallFolder() + file.getName() + "/" + configFile, "UTF-8");
                if (settings.size() == 4) {
                    jarPath = getAppInstallFolder() + file.getName() + "/" + settings.get(0).replace("jar:", "");
                    exeClass = settings.get(1).replace("class:", "");
                    exeParams = settings.get(2).replace("params:", "");
                    String cycle = settings.get(3).replace("cycle:", "").toLowerCase();
                    if (null != cycle) {
                        switch (cycle) {
                            case "once":
                                exeCycle = IApplication.ExecutePlan.ONCE;
                                break;
                            case "daily":
                                exeCycle = IApplication.ExecutePlan.DAILY;
                                break;
                            case "weekly":
                                exeCycle = IApplication.ExecutePlan.WEEKLY;
                                break;
                            case "monthly":
                                exeCycle = IApplication.ExecutePlan.MONTHLY;
                                break;
                            case "hourly":
                                exeCycle = IApplication.ExecutePlan.HOURLY;
                                break;
                            case "always":
                                exeCycle = IApplication.ExecutePlan.ALWAYS;
                                break;
                            default:
                                exeCycle = IApplication.ExecutePlan.UNKOWN;
                                break;
                        }
                    }
                }
            }
        }
        if (!"".equals(jarPath)
                && !"".equals(exeParams)
                && !"".equals(exeClass)
                && exeCycle != IApplication.ExecutePlan.UNKOWN) {
            app.setExeClass(exeClass);
            app.setJarPath(jarPath);
            app.setExecutePlan(exeCycle);
            app.setExeMethod(exeMethod);
            app.setExeParam(exeParams);
        } else {
            app.setStatus(IApplication.Status.DIED);
        }
        apps.put(app.getAppName(), app);
        return true;
    }

    /**
     * App上传目录
     */
    private String appUploadFolder;

    /**
     * @return the appUploadFolder
     */
    public String getAppUploadFolder() {
        return appUploadFolder;
    }

    /**
     * @param appUploadFolder the appUploadFolder to set
     */
    public void setAppUploadFolder(String appUploadFolder) {
        this.appUploadFolder = appUploadFolder;
        File file = new File(appUploadFolder);
        if (!file.exists()) {
            if (!file.mkdir()) {
                logger.error(appUploadFolder + " not created.");
            }
        }
    }

    /**
     * The install folder of the application.
     */
    private String appInstallFolder;

    /**
     * Get install folder.
     *
     * @return the appInstallFolder
     */
    public String getAppInstallFolder() {
        return appInstallFolder;
    }

    /**
     * Set install folder.
     *
     * @param appInstallFolder the appInstallFolder to set
     */
    public void setAppInstallFolder(String appInstallFolder) {
        this.appInstallFolder = appInstallFolder;
        File file = new File(appInstallFolder);
        if (!file.exists()) {
            if (!file.mkdir()) {
                logger.error(appInstallFolder + " not created.");
            }
        }
    }

    /**
     * Get all applicatopns.
     *
     * @return
     */
    public Collection<App> getApplications() {
        Collection<App> copyVersion = apps.values();
        return copyVersion;
    }

    /**
     * Load all applications.
     */
    public void loadApps() {
        File installFolder = new File(getAppInstallFolder());
        File[] allApps = installFolder.listFiles();
        for (File allApp : allApps) {
            if (allApp.isDirectory()) {
                addApp(allApp);
            }
        }
    }

    /**
     * Get all applications which in running.
     *
     * @return
     */
    public int getRunningAppsCount() {
        Iterator iter = apps.entrySet().iterator();
        int count = apps.size();
        while (iter.hasNext()) {
            Map.Entry entry = (Entry) iter.next();
            App app = (App) entry.getValue();
            if (app.getStatus() == IApplication.Status.JUSTINSTALL) {
                count--;
            }
        }
        return count;
    }

    /**
     * Run app.
     *
     * @param appName
     * @param dependencyApp
     * @param flag
     * @return
     */
    public String runApp(String appName, String dependencyApp, String flag) {
        if (!apps.containsKey(appName)) {
            return "Not found app " + appName;
        } else {
            if (!dependencyApp.equals("")) {
                App depApp = apps.get(dependencyApp);
                if (depApp == null) {
                    return "Dependency app does not exist.";
                }
                if (depApp.getStatus() == IApplication.Status.JUSTINSTALL) {
                    return "Dependency app does not run on this slave.";
                }
            }
            App app = apps.get(appName);
            if (app.getStatus() != IApplication.Status.RUNNING) {
                app.setDefaultParam(flag);
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd");
                try {
                    Date date = sdf.parse("2000-01-01");
                    app.setLastExeTime(date);
                } catch (ParseException e) {
                }
                app.setStatus(IApplication.Status.READY);
            }
            return "Wait the time and it will run.";
        }
    }

    /**
     * Get all applicaions in string.
     *
     * @return
     */
    public String getAllApp() {
        Iterator iter = apps.entrySet().iterator();
        StringBuilder appsInfo = new StringBuilder();
        while (iter.hasNext()) {
            Map.Entry entry = (Entry) iter.next();
            String value = entry.getValue().toString();
            appsInfo.append(value).append("\n");
        }
        return appsInfo.toString();
    }

    /**
     * Kill APP
     *
     * @param name
     * @return
     */
    public String killApp(String name) {
        boolean isKilled = Schedule.getInstance().killApp(name);
        if (isKilled) {
            apps.get(name).setStatus(IApplication.Status.KILLED);
            return "Successed.";
        } else {
            return "Failed.";
        }
    }
}
