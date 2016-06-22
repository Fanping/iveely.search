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
package com.iveely.computing.node;

import com.iveely.computing.app.IApplication;
import java.util.Date;

/**
 * Application.
 *
 * @author Iveely Liu
 */
class App {

    public App() {
        lastExeTime = new Date(1);
    }

    /**
     * Name of application.
     */
    private String name;

    /**
     * Get application's name.
     *
     * @return the name
     */
    public String getAppName() {
        return name;
    }

    /**
     * Set application's name.
     *
     * @param appName the name to set
     */
    public void setAppName(String appName) {
        this.name = appName;
    }

    /**
     * Default parameter.
     */
    private String defaultParam;

    /**
     * Size of the application.
     */
    private long size;

    /**
     * Get size of application.
     *
     * @return the size
     */
    public long getSize() {
        return size;
    }

    /**
     * @param size the size to set
     */
    public void setSize(long size) {
        this.size = size;
    }

    /**
     * Upload time of the application.
     */
    private String uploadTime;

    /**
     * Get upload time of the application.
     *
     * @return the uploadTime
     */
    public String getUploadTime() {
        return uploadTime;
    }

    /**
     * Set the upload time of the application.
     *
     * @param uploadTime the uploadTime to set
     */
    public void setUploadTime(String uploadTime) {
        this.uploadTime = uploadTime;
    }

    /**
     * Status of application.
     */
    private IApplication.Status status;

    /**
     * Get status.
     *
     * @return the status
     */
    public IApplication.Status getStatus() {
        return status;
    }

    /**
     * Set Status.
     *
     * @param status the status to set
     */
    public void setStatus(IApplication.Status status) {
        this.status = status;
    }

    /**
     * The exception of applition ,if has.
     */
    private String exception;

    /**
     * Get exception of application,if has.
     *
     * @return the exception
     */
    public String getException() {
        return exception;
    }

    /**
     * Set exception of the application,if has.
     *
     * @param exception the exception to set
     */
    public void setException(String exception) {
        this.exception = exception;
    }

    /**
     * Execution cycle.
     */
    private IApplication.ExecutePlan executePlan;

    /**
     * Get execution cycle.
     *
     * @return the executePlan
     */
    public IApplication.ExecutePlan getExecutePlan() {
        return executePlan;
    }

    /**
     * Set execution cycle.
     *
     * @param executeTime the executePlan to set
     */
    public void setExecutePlan(IApplication.ExecutePlan executeTime) {
        this.executePlan = executeTime;
    }

    /**
     * Last execute time.
     */
    private Date lastExeTime;

    /**
     * Get last execute time.
     *
     * @return the lastExeTime
     */
    public Date getLastExeTime() {
        return lastExeTime;
    }

    /**
     * Set last execute time.
     *
     * @param lastExeTime the lastExeTime to set
     */
    public void setLastExeTime(Date lastExeTime) {
        this.lastExeTime = lastExeTime;
    }

    /**
     * The start run timestamp,if has run.
     */
    private Date startExeTime;

    /**
     * Get start run timestamp,if has run.
     *
     * @return the startExeTime
     */
    public Date getStartExeTime() {
        return startExeTime;
    }

    /**
     * Get start run timestamp,if has run.
     *
     * @param startExeTime the startExeTime to set
     */
    public void setStartExeTime(Date startExeTime) {
        this.startExeTime = startExeTime;
    }

    /**
     * The main calss of the application to invoke.
     */
    private String exeClass;

    /**
     * Get main class of the application.
     *
     * @return the exeClass
     */
    public String getExeClass() {
        return exeClass;
    }

    /**
     * Set main class of the application.
     *
     * @param exeClass the exeClass to set
     */
    public void setExeClass(String exeClass) {
        this.exeClass = exeClass;
    }

    /**
     * The main method of the application to invoke.
     */
    private String exeMethod;

    /**
     * Get main method.
     *
     * @return the exeMethod
     */
    public String getExeMethod() {
        return exeMethod;
    }

    /**
     * Set main method.
     *
     * @param exeMethod the exeMethod to set
     */
    public void setExeMethod(String exeMethod) {
        this.exeMethod = exeMethod;
    }

    /**
     * The parameters to run.
     */
    private String exeParam;

    /**
     * Get parameters.
     *
     * @return the exeParam
     */
    public String getExeParam() {
        return exeParam;
    }

    /**
     * Set parameters.
     *
     * @param exeParam the exeParam to set
     */
    public void setExeParam(String exeParam) {
        this.exeParam = exeParam;
    }

    /**
     * Execute result,if has run.
     */
    private String exeResult;

    /**
     * Get execute result,if has run.
     *
     * @return the exeResult
     */
    public String getExeResult() {
        return exeResult;
    }

    /**
     * Set execute result,if has run.
     *
     * @param exeResult the exeResult to set
     */
    public void setExeResult(String exeResult) {
        this.exeResult = exeResult;
    }

    /**
     * The jar path.
     */
    private String jarPath;

    /**
     * Get jar path.
     *
     * @return the jarPath
     */
    public String getJarPath() {
        return jarPath;
    }

    /**
     * Set jar path.
     *
     * @param jarPath the jarPath to set
     */
    public void setJarPath(String jarPath) {
        this.jarPath = jarPath;
    }

    @Override
    public String toString() {
        return "app name:" + getAppName() + " " + getStatus().toString() + " " + getException();
    }

    /**
     * Get default parameters.
     *
     * @return the defaultParam
     */
    public String getDefaultParam() {
        return defaultParam;
    }

    /**
     * Set default parameters.
     *
     * @param defaultParam the defaultParam to set
     */
    public void setDefaultParam(String defaultParam) {
        this.defaultParam = defaultParam;
    }
}
