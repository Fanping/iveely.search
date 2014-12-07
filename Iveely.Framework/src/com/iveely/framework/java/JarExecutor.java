package com.iveely.framework.java;

import java.io.File;
import java.lang.reflect.InvocationTargetException;
import java.net.MalformedURLException;
import java.net.URL;
import java.net.URLClassLoader;
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
            clazz = null;
            obj = null;
            loader = null;
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
        try {
            loader = new URLClassLoader(new URL[]{file.toURI().toURL()}, this.getClass().getClassLoader());

        } catch (MalformedURLException e) {
            logger.error(e);
            return false;
        }
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
