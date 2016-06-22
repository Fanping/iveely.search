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

/**
 * Interface of read the file data.
 */
public interface IReader {

    /**
     * Initialize the reading interface. Similar as the constructor.
     *
     * @param file
     * @return True is Start-up success.
     */
    public boolean onOpen(String file);

    /**
     * Has next data to read.
     *
     * @return True is has data.
     */
    public boolean hasNext();

    /**
     * Read data by line.
     *
     * @return
     */
    public String onRead();

    /**
     * Close file.
     *
     * @return True is closed.
     */
    public boolean onClose();

}
