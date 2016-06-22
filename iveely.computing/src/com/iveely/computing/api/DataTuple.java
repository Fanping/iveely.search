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

import com.iveely.computing.api.writable.IWritable;
import java.io.Serializable;

/**
 * Used to store a series of data transmission. Essence is a serialized queue.
 */
public class DataTuple implements Serializable {

    /**
     * Serial id.
     */
    private static final long serialVersionUID = -1466479389299512377L;

    /**
     * The Key can be serialized.
     */
    public IWritable key;

    /**
     * The Value can be serialized.
     */
    public IWritable value;

    /**
     * Build tuple by two elements.
     *
     * @param key Key element.
     * @param value Value element.
     */
    public DataTuple(IWritable key, IWritable value) {
        this.key = key;
        this.value = value;
    }

    /**
     * Get key of writable.
     *
     * @return Key.
     */
    public IWritable getKey() {
        return this.key;
    }

    /**
     * Get value of writable.
     *
     * @return Value.
     */
    public IWritable getValue() {
        return this.value;
    }
}
