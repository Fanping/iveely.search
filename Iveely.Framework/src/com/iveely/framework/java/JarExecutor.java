package com.iveely.framework.java;

import java.io.File;
import java.io.UnsupportedEncodingException;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;

/**
 * Jar loader and executor.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 13:48:58
 */
public class JarExecutor {

    /**
     * Class loader.
     */
    private ClassLoader loader;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(JarExecutor.class.getName());

    /**
     * Invoke jar.
     *
     * @param jarName Jar path.
     * @param classFullName Class full name.
     * @param MethodName Method name.
     * @param paramClasses parameters.
     * @param paramValue parameters's value.
     * @return
     * @throws java.lang.Exception
     */
    public Object invoke(String jarName, String classFullName, String MethodName, Class paramClasses[], Object paramValue[]) throws Exception {
        if (!load(jarName)) {
            return new Exception(jarName + " not found.");
        }
        Class clazz = findClass(classFullName);
        Object obj;
        try {
            obj = clazz.newInstance();
            Object result = clazz.getMethod(MethodName, paramClasses).invoke(obj, paramValue);
            return result;
        } catch (InstantiationException | IllegalAccessException | NoSuchMethodException | SecurityException | IllegalArgumentException | InvocationTargetException e) {
            logger.error(e);
            throw e;
        } finally {
            loader = null;
        }
    }
    
    public String invokeJarMain(String jarName, String classFullName, String[] args) {
        if (!load(jarName)) {
            return jarName + " not found.";
        }
        try {
            Class clazz = findClass(classFullName);
            clazz.newInstance();
            Method method = clazz.getMethod("main", String[].class);
            method.invoke(null, (Object) args);
            return "OK";
        } catch (InstantiationException | IllegalAccessException | NoSuchMethodException | SecurityException | IllegalArgumentException | InvocationTargetException e) {
            logger.error(e);
            return e.getMessage();
        }
    }

    /**
     * Load jar.
     *
     * @param jar jar path.
     * @return is success.
     */
    private boolean load(String jar) {
        File file = new File(jar);
        if (!(file.exists())) {
            logger.error(jar + " not found.");
            return false;
        }
        
        String libJarPath = JarExecutor.class.getProtectionDomain().getCodeSource().getLocation().getFile();
        try {
            libJarPath = java.net.URLDecoder.decode(libJarPath, "UTF-8");
        } catch (UnsupportedEncodingException e) {
            System.out.println(e.toString());
        }
        File libFolder = new File(libJarPath).getParentFile();
        List<URL> list = new ArrayList<>();
        try {
            list.add(new File(libJarPath).toURI().toURL());
            list.add(file.toURI().toURL());
            if (libFolder.isDirectory()) {
                File[] files = libFolder.listFiles();
                for (File libFile : files) {
                    if (libFile.getName().toLowerCase().endsWith(".jar")) {
                        list.add(libFile.toURI().toURL());
                    }
                }
            }
        } catch (Exception e) {
            logger.error(e);
            return false;
        }
        URL[] urls = new URL[list.size()];
        urls = list.toArray(urls);
        loader = new URLClassLoader(urls, this.getClass().getClassLoader());
        return true;
    }

    /**
     * Find class.
     *
     * @param className
     * @return
     */
    private Class findClass(String className) {
        try {
            return loader.loadClass(className);
        } catch (ClassNotFoundException e) {
            logger.error("Class name " + className + " not found." + e);
        }
        return null;
    }
}
