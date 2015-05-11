package com.iveely.database.storage;

import com.iveely.database.common.Message;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;

/**
 *
 * @author liufanping@iveely.com
 * @date 2015-3-27 19:27:57
 */
public class IOData implements Serializable {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(IOData.class.getName());

    private final String name;

    public IOData(String name) {
        this.name = name;
    }

    /**
     * Store data.
     *
     * @param proxy
     * @param blookId
     * @return
     */
    public boolean write(Proxy proxy, int blookId) {
        Integer[] sizeSet = new Integer[proxy.getObjects().length + 1];
        int totalSize = 0;
        List<byte[]> all = new ArrayList<>();
        for (int i = 0; i < proxy.getObjects().length; i++) {
            byte[] temp = getBytesOfValue(proxy.getObjects()[i]);
            sizeSet[i + 1] = temp.length;
            totalSize += temp.length;
            all.add(temp);
        }

        int baseSize = storeBytes(all, totalSize, blookId);
        if (baseSize > -1) {
            sizeSet[0] = baseSize;
            proxy.setObjects(sizeSet);
            return true;
        }
        return false;
    }

    /**
     * Read data.
     *
     * @param proxy
     * @param blockId
     * @return
     */
    public boolean read(Proxy proxy, int blockId) {
        Object[] objs = proxy.getObjects();
        int skipSize = (Integer) objs[0];
        String fileName = this.name + "d." + blockId;
        DataInputStream in = FileStream.getDataInputStream(fileName);
        try {
            long readySkipSize = in.skip((skipSize));
            while (readySkipSize < skipSize) {
                readySkipSize += in.skip(skipSize - readySkipSize);
            }
            int readTotalSize = 0;
            Object[] readObjs = new Object[objs.length - 1];
            for (int i = 1; i < objs.length; i++) {
                int needReadSize = (Integer) objs[i];
                byte[] bytes = new byte[needReadSize];
                int actReadSize = in.read(bytes);
                while (actReadSize < needReadSize) {
                    actReadSize += in.read(bytes, actReadSize, needReadSize - actReadSize);
                }
                String data = new String(bytes, "UTF-8");
                data = data.trim();
                readObjs[i - 1] = data;
                readTotalSize += actReadSize;
            }
            in.close();
            proxy.setObjects(readObjs);
        } catch (Exception exception) {
            logger.error(exception);
        }
        return false;
    }

    /**
     * Store all bytes to disk.
     *
     * @param all
     * @return
     */
    private int storeBytes(List<byte[]> all, int totalSize, int blockId) {
        byte[] total = new byte[totalSize];
        int index = 0;
        for (byte[] all1 : all) {
            for (int j = 0; j < all1.length; j++) {
                total[index] = all1[j];
                index++;
            }
        }

        String fileName = this.name + "d." + blockId;
        DataOutputStream dbAppender = FileStream.getOutputStream(fileName);
        try {
            File file = new File(fileName);
            int baseSize = 0;
            if (file.exists()) {
                FileInputStream fis = FileStream.getInputStream(fileName);
                baseSize = fis.available();
            }
            dbAppender.write(total);
            return baseSize;
        } catch (Exception e) {
            logger.error(e);
            return -1;
        }
    }

    /**
     * Get bytes of the value.
     *
     * @param obj
     * @return
     */
    private byte[] getBytesOfValue(Object obj) {
        return Message.getBytes((String) obj);
    }
}
