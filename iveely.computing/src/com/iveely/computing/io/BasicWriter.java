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
package com.iveely.computing.io;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.OutputStreamWriter;
import java.io.UnsupportedEncodingException;

import org.apache.log4j.Logger;

/**
 * Basic file write operation, the api is based on the basic of the operating
 * system.
 */
public class BasicWriter implements IWriter {

    private final Logger logger = Logger.getLogger(BufferedWriter.class);

    /**
     * Buffer writer.
     */
    private BufferedWriter writer;

    /**
     * String buffer to cache data.
     */
    private StringBuffer buffer;

    /**
     * The cache max size.
     */
    private final int max;

    /**
     * The cursor of buffer.
     */
    private int cursor;

    /**
     * Build basic writer.
     */
    public BasicWriter() {
        this.max = 1000;
        this.cursor = 0;
        this.buffer = new StringBuffer();
    }

    /*
     * (non-Javadoc)
     * 
     * @see com.iveely.computing2.io.IWriter#onOpen(java.io.File)
     */
    @Override
    public boolean onOpen(String filePath) {
        try {
            File file = new File(filePath);
            writer = new BufferedWriter(
                    new OutputStreamWriter(new FileOutputStream(file, true), "UTF-8"));
            return true;
        } catch (FileNotFoundException | UnsupportedEncodingException e) {
            logger.error("When open file on basic writer,exception happend.", e);
        }
        return false;
    }

    /*
     * (non-Javadoc)
     * 
     * @see com.iveely.computing2.io.IWriter#onWrite()
     */
    @Override
    public boolean onWrite(String text) {
        try {
            if (this.cursor == this.max) {
                this.cursor = 0;
                this.writer.write(buffer.toString());
                this.buffer.setLength(0);
            }
            this.buffer.append(text);
            this.buffer.append("\n");
            this.cursor++;
            return true;
        } catch (Exception e) {
            logger.error("When write file on basic writer,exception happend.", e);
        }
        return false;
    }

    /*
     * (non-Javadoc)
     * 
     * @see com.iveely.computing2.io.IWriter#onClose()
     */
    @Override
    public boolean onClose() {
        try {
            if (buffer.length() != 0 && writer != null) {
                writer.write(buffer.toString());
                buffer = new StringBuffer();
            }
            if (writer != null) {
                writer.close();
                writer = null;
            }
            return true;
        } catch (Exception e) {
            logger.error("When close file on basic writer,exception happend.", e);
        }
        return false;
    }
}
