/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.database.storage;

import java.io.BufferedInputStream;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.util.TreeMap;
import org.apache.log4j.Logger;
/**
 *
 * @author X1 Carbon
 */
public class FileStream {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(FileStream.class.getName());

    private static final TreeMap<String, DataOutputStream> outputStreams = new TreeMap<>();

    private static final TreeMap<String, FileInputStream> fileInputStreams = new TreeMap<>();

    private static final TreeMap<String, DataInputStream> dataInputStreams = new TreeMap<>();

    public static DataOutputStream getOutputStream(String filePath) {
        if (outputStreams.containsKey(filePath)) {
            return outputStreams.get(filePath);
        } else {
            try {
                DataOutputStream dbAppender = new DataOutputStream(new FileOutputStream(filePath, true));
                outputStreams.put(filePath, dbAppender);
                return dbAppender;
            } catch (Exception e) {
                logger.error(e);
            }
        }
        return null;
    }

    public static FileInputStream getInputStream(String filePath) {
        if (fileInputStreams.containsKey(filePath)) {
            return fileInputStreams.get(filePath);
        } else {
            try {
                FileInputStream inputStream = new FileInputStream(filePath);
                fileInputStreams.put(filePath, inputStream);
                return inputStream;
            } catch (Exception e) {
                logger.error(e);
            }
        }
        return null;
    }

    public static DataInputStream getDataInputStream(String filePath) {
        try {
            DataInputStream in = new DataInputStream(
                    new BufferedInputStream(
                            new FileInputStream(filePath)));
            dataInputStreams.put(filePath, in);
            return in;
        } catch (FileNotFoundException ex) {
          logger.error(ex);
        }
        return null;
    }

}
