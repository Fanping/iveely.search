/*
 * Copyright 2016 liufanping@iveely.com.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.iveely.framework.compile;

import org.apache.log4j.Logger;

import java.io.File;
import java.io.UnsupportedEncodingException;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

/**
 * Jar loader and executor.
 *
 * @author liufanping (liufanping@iveely.com)
 */
public class JarExecutor {

  /**
   * Logger.
   */
  private final Logger logger = Logger.getLogger(JarExecutor.class.getName());

  /**
   * Class loader.
   */
  private ClassLoader loader;

  /**
   * Invoke jar.
   *
   * @param jarName       Jar path.
   * @param classFullName Class full name.
   * @param MethodName    Method name.
   * @param paramClasses  parameters.
   * @param paramValue    parameters's value.
   */
  public Object invoke(String jarName, String classFullName, String MethodName, Class paramClasses[],
                       Object paramValue[]) throws Exception {
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

  public boolean isClassExist(String jarName, String classFullName) {
    if (!load(jarName)) {
      return false;
    }
    Class clazz = findClass(classFullName);
    if (clazz == null) {
      return false;
    }
    return true;

  }

  public String invokeJarMain(String jarName, String classFullName, String[] args) {
    if (!load(jarName)) {
      return jarName + " not found.";
    }
    try {
      Class clazz = findClass(classFullName);
      Method method = clazz.getMethod("main", String[].class);
      method.invoke(null, (Object) args);
      return "OK";
    } catch (IllegalAccessException | NoSuchMethodException | SecurityException | IllegalArgumentException | InvocationTargetException e) {
      e.printStackTrace();
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
          if (libFile.getName().toLowerCase(Locale.CHINESE).endsWith(".jar")) {
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
