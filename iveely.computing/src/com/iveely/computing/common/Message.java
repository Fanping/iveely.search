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
package com.iveely.computing.common;

import java.io.UnsupportedEncodingException;
import java.nio.charset.Charset;

/**
 * The transfer message in iveely computing.
 *
 * @author Iveely Liu
 */
public class Message {

  private Message() {
  }

  /**
   * Get execute type.
   */
  public static ExecuteType getExecuteType(int index) {
    return ExecuteType.valueOf(index);
  }

  /**
   * Get mime type.
   */
  public static MIMEType getMIMEType(int index) {
    return MIMEType.valueOf(index);
  }

  /**
   * Convert string to byte[].
   */
  public static byte[] getBytes(String content) {
    byte[] bytes;
    try {
      bytes = content.getBytes("UTF-8");
    } catch (UnsupportedEncodingException ex) {
      bytes = content.getBytes(Charset.defaultCharset());
    }
    return bytes;
  }

  /**
   * Convert byte[] to string.
   */
  public static String getString(byte[] bytes) {
    try {
      return new String(bytes, "UTF-8").trim();
    } catch (UnsupportedEncodingException ex) {
      return new String(bytes, Charset.defaultCharset()).trim();
    }
  }

  /**
   * Execute type.
   */
  public enum ExecuteType {

    /**
     * Heartbeat.
     */
    HEARTBEAT(0),
    /**
     * Response heartbeat.
     */
    RESPHEARTBEAT(1),
    /**
     * Kill task.
     */
    KILLTASK(2),
    /**
     * list all tasks.
     */
    LIST(3),
    /**
     * upload app to iveely computing.
     */
    UPLOAD(4),
    /**
     * Delete file from iveely.computing.
     */
    DELETEFILE(5),
    /**
     * Rename file from iveely.computing.
     */
    RENAMEFILE(6),
    /**
     * List all files.
     */
    LISTFILE(7),
    /**
     * Show disk left.
     */
    SHOWDISK(8),
    /**
     * Show memory used.
     */
    SHOWMEMORY(9),
    /**
     * Show all slaves with tasks' count.
     */
    SLAVES(10),
    /**
     * Response all slaves.
     */
    RESPSLAVES(11),
    /**
     * Respnse upload result.
     */
    RESPUPLOADAPP(12),
    /**
     * Execute app.
     */
    RUN(13),
    /**
     * Response result of execute.
     */
    RESPRUNAPP(14),
    /**
     * Response all tasks.
     */
    RESPLISTTASK(15),
    /**
     * Response result of kill task.
     */
    RESPKILLTASK(16),
    /**
     * Set distribute memory cache.
     */
    SETCACHE(17),
    /**
     * Response result of set cache.
     */
    RESPSETCACHE(18),
    /**
     * Append cache.
     */
    APPENDCACHE(19),
    /**
     * Response result of append cache.
     */
    RESPAPPENDCACHE(20),
    /**
     * Get cache.
     */
    GETCACHE(21),
    /**
     * Response result of get cache.
     */
    RESPGETCACHE(22),
    /**
     * Regist the distribute cache key.
     */
    REGISTE(23),
    /**
     * Response result of registe the cache.
     */
    RESPREGIST(24),
    /**
     * Callback of key change event.
     */
    CALLBACKOFKEYEVENT(25),
    /**
     * Response result of key change event.
     */
    RESPCALLBACKOFKEYEVENT(26),
    /**
     * Check worker is online.
     */
    ISONLINE(27),
    /**
     * Response result of online check.
     */
    RESPISONLINE(28),
    /**
     * Rebalance topology.
     */
    REBALANCE(29),
    /**
     * Response result of rebalance topology.
     */
    RESPREBALANCE(30),
    /**
     * Unknown.
     */
    UNKOWN(999);

    private int value = 0;

    private ExecuteType(int value) {
      this.value = value;
    }

    /**
     * Get execute type by value.
     *
     * @param value such as 1.
     */
    public static ExecuteType valueOf(int value) {
      switch (value) {
        case 0:
          return HEARTBEAT;
        case 1:
          return RESPHEARTBEAT;
        case 2:
          return KILLTASK;
        case 3:
          return LIST;
        case 4:
          return UPLOAD;
        case 5:
          return DELETEFILE;
        case 6:
          return RENAMEFILE;
        case 7:
          return LISTFILE;
        case 8:
          return SHOWDISK;
        case 9:
          return SHOWMEMORY;
        case 10:
          return SLAVES;
        case 11:
          return RESPSLAVES;
        case 12:
          return RESPUPLOADAPP;
        case 13:
          return RUN;
        case 14:
          return RESPRUNAPP;
        case 15:
          return RESPLISTTASK;
        case 16:
          return RESPKILLTASK;
        case 17:
          return SETCACHE;
        case 18:
          return RESPSETCACHE;
        case 19:
          return APPENDCACHE;
        case 20:
          return RESPAPPENDCACHE;
        case 21:
          return GETCACHE;
        case 22:
          return RESPGETCACHE;
        case 23:
          return REGISTE;
        case 24:
          return RESPREGIST;
        case 25:
          return CALLBACKOFKEYEVENT;
        case 26:
          return RESPCALLBACKOFKEYEVENT;
        case 27:
          return ISONLINE;
        case 28:
          return RESPISONLINE;
        case 29:
          return REBALANCE;
        case 30:
          return RESPREBALANCE;
        default:
          return UNKOWN;
      }
    }

    /**
     * Get execute type by name.
     *
     * @param name such as "RUN".
     */
    public static ExecuteType valueOfName(String name) {
      switch (name) {
        case "HEARTBEAT":
          return HEARTBEAT;
        case "RESPHEARTBEAT":
          return RESPHEARTBEAT;
        case "KILL":
          return KILLTASK;
        case "LIST":
          return LIST;
        case "UPLOAD":
          return UPLOAD;
        case "DELETEFILE":
          return DELETEFILE;
        case "RENAMEFILE":
          return RENAMEFILE;
        case "LISTFILE":
          return LISTFILE;
        case "SHOWDISK":
          return SHOWDISK;
        case "SHOWMEMORY":
          return SHOWMEMORY;
        case "SLAVES":
          return SLAVES;
        case "RESPSLAVES":
          return RESPSLAVES;
        case "RESPUPLOADAPP":
          return RESPUPLOADAPP;
        case "RUN":
          return RUN;
        case "RESPRUNAPP":
          return RESPRUNAPP;
        case "RESPLISTTASK":
          return RESPLISTTASK;
        case "RESPKILLTASK":
          return RESPKILLTASK;
        case "SETCACHE":
          return SETCACHE;
        case "RESPSETCACHE":
          return RESPSETCACHE;
        case "APPENDCACHE":
          return APPENDCACHE;
        case "RESPAPPENDCACHE":
          return RESPAPPENDCACHE;
        case "GETCACHE":
          return GETCACHE;
        case "RESPGETCACHE":
          return RESPGETCACHE;
        case "REGISTE":
          return REGISTE;
        case "RESPREGIST":
          return RESPREGIST;
        case "CALLBACKOFKEYEVENT":
          return CALLBACKOFKEYEVENT;
        case "RESPCALLBACKOFKEYEVENT":
          return RESPCALLBACKOFKEYEVENT;
        case "ISONLINE":
          return ISONLINE;
        case "RESPISONLINE":
          return RESPISONLINE;
        case "REBALANCE":
          return REBALANCE;
        case "RESPREBALANCE":
          return RESPREBALANCE;
        default:
          return UNKOWN;
      }
    }
  }

  /**
   * The mime type of media
   */
  public enum MIMEType {

    /**
     * Text.
     */
    TEXT(0),
    /**
     * Application.
     */
    APP(1),
    /**
     * Normal message.
     */
    MESSAGE(2),
    /**
     * Unknown.
     */
    UNKOWN(999);

    private int value = 0;

    private MIMEType(int value) {
      this.value = value;
    }

    /**
     * Get mime type by value.
     *
     * @param value such as "APP".
     */
    public static MIMEType valueOf(int value) {
      switch (value) {
        case 0:
          return TEXT;
        case 1:
          return APP;
        case 2:
          return MESSAGE;
        default:
          return UNKOWN;
      }
    }
  }
}
