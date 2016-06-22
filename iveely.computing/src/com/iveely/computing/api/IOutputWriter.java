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
package com.iveely.computing.api;

import com.iveely.computing.io.IWriter;
import com.iveely.computing.task.WriterTask;
import java.util.HashMap;

/**
 * OutputWriter is an IOutput that can be written.
 *
 * is a subclass of IOuput, used for writing data to a local file system or
 * other file system.
 */
public abstract class IOutputWriter extends IOutput {

    /**
     * Data writing tools.
     */
    private IWriter writer;

    /**
     * Whether the file is already open properly.
     */
    private boolean isOpenProperly;

    /**
     * Build output writer instance..
     */
    public IOutputWriter() {
        super();
        this.isOpenProperly = false;
    }

    /**
     * @see IOutput#start(java.util.HashMap)
     * @param conf The user's custom configuration information.
     */
    @Override
    public void start(HashMap<String, Object> conf) {
        WriterTask writerTask = (WriterTask) conf.get(this.getClass().getName());
        try {
            this.writer = writerTask.getWriter().getClass().newInstance();
        } catch (InstantiationException | IllegalAccessException ex) {

        }
        this.isOpenProperly = this.writer.onOpen(writerTask.getFile());
    }

    @Override
    public void execute(DataTuple tuple, StreamChannel channel) {
        if (this.isOpenProperly) {
            execute(tuple, channel, writer);
        } else {
            execute(tuple, channel, null);
        }
    }

    /**
     * Process recived tuple with writer.
     *
     * @param tuple The data tuple.
     * @param channel Stream channel.
     * @param writer Data writing tools.
     */
    public abstract void execute(DataTuple tuple, StreamChannel channel, IWriter writer);

    /**
     * @see IOutput#end(java.util.HashMap)
     *
     * @param conf The user's custom configuration information.
     */
    @Override
    public void end(HashMap<String, Object> conf) {
        this.end(conf, this.writer);
        if (this.writer != null) {
            this.writer.onClose();
        }
    }

    /**
     * Prepare before execute.
     *
     * @param conf Customized information.
     * @param writer Data writing tools.
     */
    public abstract void end(HashMap<String, Object> conf, IWriter writer);

}
