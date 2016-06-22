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
package com.iveely.computing.task;

import com.iveely.computing.io.IReader;
import java.util.List;

/**
 * Reader task,Distribution of files to the reader.
 */
public class ReaderTask {

    private IReader reader;

    private List<String> files;

    /**
     * Build reader task.
     *
     * @param reader The reader to read.
     * @param files All files assigned to the reader.
     */
    public ReaderTask(IReader reader, List<String> files) {
        this.reader = reader;
        this.files = files;
    }

    /**
     * @return the reader
     */
    public IReader getReader() {
        return reader;
    }

    /**
     * @param reader the reader to set
     */
    public void setReader(IReader reader) {
        this.reader = reader;
    }

    /**
     * @return the files
     */
    public List<String> getFiles() {
        return files;
    }

    /**
     * @param files the files to set
     */
    public void setFiles(List<String> files) {
        this.files = files;
    }
}
