package com.iveely.framework.file;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;

/**
 * File reader.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 21:22:12
 */
public class Reader {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Reader.class.getName());

    /**
     * Read file to string.
     *
     * @param file
     * @param encoding
     * @return
     */
    public static String readToString(File file, String encoding) {
        Long filelength = file.length();
        byte[] filecontent = new byte[filelength.intValue()];
        FileInputStream in = null;
        try {
            in = new FileInputStream(file);
            in.read(filecontent);
            in.close();
        } catch (FileNotFoundException e) {
            logger.error(e);
        } catch (IOException e) {
            logger.error(e);
        } finally {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException e) {
                    logger.error(e);
                }
            }
        }
        try {
            return new String(filecontent, encoding);
        } catch (UnsupportedEncodingException e) {
            return new String(filecontent);
        }
    }

    /**
     * Read file to lines.
     *
     * @param filePath
     * @param encoding
     * @return
     */
    public static List<String> readAllLine(String filePath, String encoding) {
        List<String> lines = new ArrayList<>();
        try {
            File file = new File(filePath);
            if (file.isFile() && file.exists()) {
                InputStreamReader read;
                read = new InputStreamReader(new FileInputStream(file), encoding);
                try (BufferedReader bufferedReader = new BufferedReader(read)) {
                    String lineTxt = null;
                    while ((lineTxt = bufferedReader.readLine()) != null) {
                        if (lineTxt.length() > 1) {
                            lines.add(lineTxt.trim());
                        }
                    }
                }
                read.close();
            } else {
                logger.error(filePath + " not found.");
            }
        } catch (IOException e) {
            logger.error(e);
         }
        return lines;
    }
}
