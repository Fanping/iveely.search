package com.iveely.computing.common;

import java.io.UnsupportedEncodingException;
import java.nio.charset.Charset;

/**
 * The transfer message in iveely computing.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 20:33:16
 */
public class Message {

    /**
     * Execute type.
     */
    public enum ExecutType {

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
        RESPSETCACHE(18),
        /**
         * Append cache.
         */
        APPENDCACHE(19),
        RESPAPPENDCACHE(20),
        /**
         * Get cache.
         */
        GETCACHE(21),
        RESPGETCACHE(22),
        /**
         * Regist the distribute cache key.
         */
        REGISTE(23),
        RESPREGIST(24),
        /**
         * Callback of key change event.
         */
        CALLBACKOFKEYEVENT(25),
        RESPCALLBACKOFKEYEVENT(26),
        /**
         * Unknown.
         */
        UNKOWN(999);

        private int value = 0;

        private ExecutType(int value) {
            this.value = value;
        }

        /**
         * Get execute type by value.
         *
         * @param value such as 1.
         * @return
         */
        public static ExecutType valueOf(int value) {
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
                default:
                    return UNKOWN;
            }
        }

        /**
         * Get execute type by name.
         *
         * @param name such as "RUN".
         * @return
         */
        public static ExecutType valueOfName(String name) {
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
         * @return
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

    /**
     * Get execute type.
     *
     * @param index
     * @return
     */
    public static ExecutType getExecuteType(int index) {
        return ExecutType.valueOf(index);
    }

    /**
     * Get mime type.
     *
     * @param index
     * @return
     */
    public static MIMEType getMIMEType(int index) {
        return MIMEType.valueOf(index);
    }

    /**
     * Convert string to byte[].
     *
     * @param content
     * @return
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
     *
     * @param bytes
     * @return
     */
    public static String getString(byte[] bytes) {
        try {
            return new String(bytes, "UTF-8").trim();
        } catch (UnsupportedEncodingException ex) {
            return new String(bytes, Charset.defaultCharset()).trim();
        }
    }
}
