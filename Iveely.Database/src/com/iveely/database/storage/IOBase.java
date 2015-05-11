package com.iveely.database.storage;

import com.iveely.database.common.Common;
import com.iveely.database.common.Convertor;
import java.io.BufferedInputStream;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.io.Serializable;
import java.nio.channels.FileChannel;
import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;

/**
 * Object write.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 22:00:53
 */
public class IOBase implements Serializable {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(IOBase.class.getName());

    /**
     * Database's name.
     */
    private String dbName;

    /**
     * Root folder of data.
     */
    private final String root;

    /**
     * The object which be stored.
     */
    private final Proxy proxy;

    /**
     * Size of the header.
     */
    private final int headerSize;

    /**
     * Current block id.
     */
    private int currentBlockId;

    /**
     * Size of each record.
     */
    private int recordSize;

    /**
     * The name of the table.
     */
    private final String tableName;

    /**
     * Max record size.
     */
    private final Integer maxRecodSize = 100 * 10000;

    /**
     * The data write.
     */
    private final IOData dataStore;

    /**
     * Total count of data.
     */
    private Integer totalCount;

    /**
     * All types of columns.
     */
    private final Types[] types;
    
    public IOBase(String dbName, String tableName, Proxy proxy, Types[] types) {
        this.dbName = dbName;
        this.proxy = proxy;
        this.tableName = tableName;
        this.types = types;
        this.root = "Warehouses";
        this.dataStore = new IOData(this.root + "/" + this.dbName + "/" + this.tableName + "/");

        // 1. Each record's size.
        this.recordSize = 0;
        this.totalCount = 0;
        this.recordSize = 4 * (types.length + 1);

        // 2. Header size.
        headerSize = Common.getDefaultSize() * 5 + (this.types.length + 1) * Common.getDefaultSize();

        // 3.  Init block.
        currentBlockId = 0;
    }

    /**
     * Write an object to hard disk.
     *
     * @param proxy
     * @return
     * @throws FileNotFoundException
     * @throws IOException
     * @throws IllegalArgumentException
     */
    public int write(Proxy proxy) throws FileNotFoundException, IOException, IllegalArgumentException {
        if (proxy == null) {
            return -1;
        }
        Proxy[] objs = new Proxy[1];
        objs[0] = proxy;
        return write(objs);
    }

    /**
     * Write many objects to hard disk.
     *
     * @param proxies
     * @return
     */
    public int write(Proxy[] proxies) {
        synchronized (this) {
            if (proxies == null) {
                return -1;
            }
            String rootFile = "data";
            if (dbName.length() > 0) {
                rootFile = this.root + "/" + this.dbName + "/" + this.tableName + "/" + rootFile;
            }

            // 0. Current count.
            currentBlockId = totalCount / maxRecodSize;
            int blockFlag = currentBlockId + 1;//currentBlockId == 0 ? 1 : currentBlockId;
            String fileName = rootFile + "." + blockFlag;
            for (int i = 0; i < proxies.length; i++) {
                proxies[i].setId(totalCount + i);
            }

            // 1. Current data file left capacity.
            int capacity = maxRecodSize - ((totalCount < 0 ? 0 : totalCount)) % maxRecodSize;
//            if (capacity == maxRecodSize && totalCount != 0) {
//                blockFlag += 1;
//                fileName = rootFile + "." + blockFlag;
//            }

            // 2. If not need create new data file.
            if (proxies.length <= capacity) {
                return writeObjects(proxies, blockFlag);
            } else {

                // 3. Create new data file.
                int currentIndex = -1;
                Proxy[] currentObjs = new Proxy[capacity];
                for (int i = 0; i < capacity; i++) {
                    currentObjs[i] = proxies[i];
                    currentIndex++;
                }
                if (capacity > 0) {
                    writeObjects(currentObjs, blockFlag);
                }
                int lastFlag = -1;
                int size = (proxies.length - capacity) / maxRecodSize;
                for (int j = 0; j < size; j++) {
                    currentObjs = new Proxy[maxRecodSize];
                    for (int i = 0; i < maxRecodSize; i++) {
                        currentObjs[i] = proxies[capacity + j * maxRecodSize + i];
                        currentIndex++;
                    }
                    lastFlag = writeObjects(currentObjs, blockFlag);
                }
                currentIndex++;
                int leftCapacity = proxies.length - currentIndex;
                if (leftCapacity > 0) {
                    currentObjs = new Proxy[leftCapacity];
                    for (int i = 0; i < leftCapacity; i++) {
                        currentObjs[i] = proxies[currentIndex + i];
                    }
                    return writeObjects(currentObjs, blockFlag);
                } else {
                    return lastFlag;
                }
            }
        }
    }

    /**
     * Read an object from hard disk.
     *
     * @param index
     * @return
     */
    public Proxy read(int index) {
        synchronized (this) {
            // 1. Get file name.
            String rootFile = "data";
            if (dbName.length() > 0) {
                rootFile = this.root + "/" + this.dbName + "/" + this.tableName + "/" + rootFile;
            }

            // 2. Get current record size.
            currentBlockId = index / maxRecodSize;
            int blockFlag = currentBlockId == 0 ? 1 : currentBlockId + 1;
            String fileName = rootFile + "." + blockFlag;
            File dbFile = new File(fileName);
            if (!dbFile.exists()) {
                System.out.println(fileName + " not found.");
                return null;
            }
            index = index % maxRecodSize;

            // 3. Calculate skip size.
            int skipSize = headerSize + recordSize * index;

            // 4. Read data.
            try {
                Proxy buildObject = new Proxy();
                DataInputStream in = FileStream.getDataInputStream(fileName);
                {
                    long readySkipSize = in.skip(skipSize);
                    while (readySkipSize < skipSize) {
                        readySkipSize += in.skip(skipSize - readySkipSize);
                    }

                    // 4.1 Read id.
                    int needReadSize = 4;//getTypeSize(Types.INTEGER);
                    byte[] bytes;
                    int actReadSize;

                    // 4.2 Read object[];
                    int readTotalSize = 0;
                    Object[] objs = new Object[types.length + 1];
                    for (int i = 0; i < types.length + 1; i++) {
                        bytes = new byte[needReadSize];
                        actReadSize = in.read(bytes);
                        while (actReadSize < needReadSize) {
                            actReadSize += in.read(bytes, actReadSize, needReadSize - actReadSize);
                        }
                        objs[i] = Convertor.bytesToInt(bytes);
                        readTotalSize += actReadSize;
                    }
                    in.close();
                    //   in.skipBytes((readTotalSize + (int) readySkipSize) * -1);
                    buildObject.setObjects(objs);
                    dataStore.read(buildObject, blockFlag);
                }
                return buildObject;
            } catch (Exception e) {
                logger.error(e);
            }
            return null;
        }
    }

    /**
     * Update data on hard disk.
     *
     * @param index index location.
     * @param obj new object.
     * @return
     */
    public boolean update(int index, Proxy obj) {

        // 1. Get file name.
        String rootFile = "data";
        if (dbName.length() > 0) {
            rootFile = this.root + "/" + this.dbName + "/" + this.tableName + "/" + rootFile;
        }

        // 2. Current count of data.
        int blockFlag = currentBlockId == 0 ? 1 : currentBlockId;
        String fileName = rootFile + "." + ((index) / maxRecodSize + 1);
        index = index % maxRecodSize;

        // 3. Calculate skip size.
        int skipSize = headerSize + recordSize * index;

        // 4. Update objecet.
        try {
            boolean isUpdateSuccess = false;
            try (RandomAccessFile randomAccessFile = new RandomAccessFile(fileName, "rw")) {
                randomAccessFile.seek(skipSize);
                byte[] singleObj = getObjBytes(obj);
                if (singleObj != null) {
                    randomAccessFile.write(singleObj);
                    isUpdateSuccess = true;
                }
                randomAccessFile.close();
            }
            return isUpdateSuccess;
        } catch (IOException e) {
            logger.error(e);
        }
        return false;
    }

    /**
     * Get name of database.
     *
     * @return
     */
    public String getDbName() {
        return this.dbName;
    }

    /**
     * Get count of data.
     *
     * @return
     */
    public int getCount() {
        return this.totalCount;
    }

    /**
     * Rename database.
     *
     * @param dbName
     */
    public void setDBName(String dbName) {
        this.dbName = dbName;
    }

    /**
     * Get total count of the data write.
     *
     * @param fileName
     * @return
     */
    private int getTotalCount(String fileName) {
        int count = -1;
        try {
            File file = new File(fileName);
            if (!file.exists()) {
                return count;
            }
            try (DataInputStream in = new DataInputStream(new BufferedInputStream(new FileInputStream(fileName)))) {
                int avaid = in.available();
                if (avaid > 0) {
                    byte[] bytes = new byte[Common.getDefaultSize()];
                    in.skip(Common.getDefaultSize() * 2);
                    in.read(bytes);
                    count = Convertor.bytesToInt(bytes);
                } else {
                    count = 0;
                }
            }
        } catch (IOException e) {
            logger.error(e);
        }
        return count;
    }

    /**
     * Get file block count.
     *
     * @param fileName
     * @return
     */
    private int getFileBlockCount(String fileName) {
        int count = 0;
        try {
            File file = new File(fileName);
            if (!file.exists()) {
                return count;
            }
            try (DataInputStream in = new DataInputStream(new BufferedInputStream(new FileInputStream(fileName)))) {
                int avaid = in.available();
                if (avaid > 0) {
                    byte[] bytes = new byte[Common.getDefaultSize()];
                    in.skip(Common.getDefaultSize());
                    in.read(bytes);
                    count = Convertor.bytesToInt(bytes);
                } else {
                    count = 0;
                }
            }
            
        } catch (IOException e) {
            logger.error(e);
        }
        return count;
    }

    /**
     * Modify partial data in a file.
     *
     * @param fileName
     * @param start
     * @param replaceBs
     * @return
     * @throws Exception
     */
    private boolean modifyFile(String fileName, int start, byte[] replaceBs) throws Exception {
        java.nio.channels.FileChannel channel;
        try (java.io.RandomAccessFile raf = new java.io.RandomAccessFile(fileName, "rw")) {
            channel = raf.getChannel();
            java.nio.MappedByteBuffer buffer = channel.map(FileChannel.MapMode.READ_WRITE, start, replaceBs.length);
            for (int i = 0; i < replaceBs.length; i++) {
                byte src = buffer.get(i);
                buffer.put(i, replaceBs[i]);
            }
            buffer.force();
            buffer.clear();
            channel.close();
        }
        return true;
    }

    /**
     * Notify all file block.
     *
     * @param fileName
     * @param count
     * @param value
     */
    private void notifyAddBlock(String rootFile, int count) {
        try {
            for (int i = 1; i <= count; i++) {
                String fileNameString = rootFile + "." + i;
                byte[] countBytes = Convertor.int2byte(count);
                modifyFile(fileNameString, Common.getDefaultSize(), countBytes);
            }
        } catch (Exception e) {
            logger.error(e);
        }
    }

    /**
     * Convert an object to bytes.
     *
     * @param obj
     * @return
     */
    private byte[] getObjBytes(Proxy obj) {
        if (obj == null) {
            return null;
        }
        int totalSize = 0;
        List<byte[]> all = new ArrayList<>();
        for (Object object : obj.getObjects()) {
            byte[] temp = Convertor.int2byte((Integer) object);
            all.add(temp);
            totalSize += temp.length;
        }
        
        byte[] total = new byte[totalSize];
        int index = 0;
        for (byte[] all1 : all) {
            for (int j = 0; j < all1.length; j++) {
                total[index] = all1[j];
                index++;
            }
        }
        return total;
    }

    /**
     * Write many objects.
     *
     * @param proxies
     * @return
     */
    private int writeObjects(Proxy[] proxies, int blockFlag) {
        if (proxies == null) {
            return -1;
        }
        try {
            String rootFile = "data";
            if (dbName.length() > 0) {
                rootFile = this.root + "/" + this.dbName + "/" + this.tableName + "/" + rootFile;
            }

            // 1. Get count of the data.
            String fileName = rootFile + "." + blockFlag;
          //  int count = this.totalCount;//getTotalCount(fileName);

            // 2. If first write data.
            if ((this.totalCount) % maxRecodSize == 0) {
                fileName = rootFile + "." + blockFlag;

                //---------------------------------------------------------------------------------------------------------
                // 4byte:version| 4byte:当前文件块数 | 4byte：表记录数 | 4byte:Header长度 | 4byte:表标识 | cloumntype(4byte) | 
                //--------------------------------------------------------------------------------------------------------
                // 2.1 Write data.
                DataOutputStream out = FileStream.getOutputStream(fileName);
                {

                    // 2.1.1 Write current version of data.
                    byte[] version = Convertor.int2byte(8);
                    out.write(version);

                    // 2.1.2 Write file block count.
                    byte[] blocks = Convertor.int2byte(blockFlag);
                    out.write(blocks);

                    // 2.1.3 Write table's record area.
                    byte[] tabLength = Convertor.int2byte(0);
                    out.write(tabLength);

                    // 2.1.4 Write header's length.
                    byte[] headerSizeBytes = Convertor.int2byte(headerSize);
                    out.write(headerSizeBytes);

                    // 2.1.5 Write table's name.
                    String tabName = tableName;
                    byte[] realBytes = Convertor.int2byte(tabName.hashCode());
                    out.write(realBytes);

                    // 2.1.6 Write clumn's type.
                    byte[] bytes = Convertor.int2byte(Types.INTEGER.ordinal());
                    out.write(bytes);
                    for (Types type : types) {
                        bytes = Convertor.int2byte(type.ordinal());
                        out.write(bytes);
                    }
                }
                //notifyAddBlock(rootFile, blockFlag);
            }

            // 3. Write data.
            boolean isWriteSuccess = false;
            DataOutputStream dbAppender = FileStream.getOutputStream(fileName);
            {
                List<byte[]> list = new ArrayList();
                byte[] singleObj = null;
                if (dataStore.write(proxies[0], blockFlag)) {
                    singleObj = getObjBytes(proxies[0]);
                    if (singleObj != null) {
                        list.add(singleObj);
                    }
                }
                for (int m = 1; m < proxies.length; m++) {
                    if (dataStore.write(proxies[m], blockFlag)) {
                        singleObj = getObjBytes(proxies[m]);
                        if (singleObj != null) {
                            list.add(singleObj);
                        }
                    }
                }
                if (proxies.length > 1 && singleObj != null) {
                    byte[][] totalObjBytes = new byte[list.size()][singleObj.length];
                    totalObjBytes = list.toArray(totalObjBytes);
                    byte[] totalBytes = new byte[list.size() * singleObj.length];
                    for (int k = 0; k < list.size(); k++) {
                        System.arraycopy(totalObjBytes[k], 0, totalBytes, k * singleObj.length, singleObj.length);
                    }
                    dbAppender.write(totalBytes);
                    isWriteSuccess = true;
                } else {
                    if (singleObj != null) {
                        dbAppender.write(singleObj);
                        isWriteSuccess = true;
                    }
                }
            }

            // 4. Record count+1
            if (isWriteSuccess) {
                this.totalCount += proxies.length;
                return this.totalCount;
            } else {
                return -1;
            }
        } catch (Exception e) {
            logger.error(e);
        }
        return -1;
    }

    /**
     * Convert header type to bytes.
     *
     * @param typeString
     * @return
     */
    private byte[] getHeaderType(String typeString) {
        int headerType = 0;
        typeString = typeString.toLowerCase();
        switch (typeString) {
            case "integer":
            case "int":
                headerType = Types.INTEGER.ordinal();
                break;
            case "double":
                headerType = Types.DOUBLE.ordinal();
                break;
            case "float":
                headerType = Types.FLOAT.ordinal();
                break;
            case "long":
                headerType = Types.LONG.ordinal();
                break;
            case "char":
            case "character":
                headerType = Types.CHAR.ordinal();
                break;
            case "string":
                headerType = Types.STRING.ordinal();
                break;
            case "shortstring":
                headerType = Types.SHORTSTRING.ordinal();
                break;
            case "boolean":
            case "bool":
                headerType = Types.BOOLEAN.ordinal();
                break;
            case "base64image":
                headerType = Types.IMAGE.ordinal();
                break;
            default:
                System.out.println("Not found any type," + typeString);
        }
        return Convertor.int2byte(headerType);
    }

    /**
     * Calculation size of field.
     *
     * @param field
     * @return
     */
    private int getTypeSize(Types type) {
        int valSize = Common.getSimpleStringSize();
        if (type == Types.STRING) {
            valSize = Common.getLongStringSize();
        }
        if (type == Types.SHORTSTRING) {
            valSize = Common.getShortStringSize();
        }
        if (type == Types.IMAGE) {
            valSize = Common.getBase64StringSize();
        }
        if (type == Types.BOOLEAN) {
            valSize = Common.getBooleanSize();
        }
        if (type == Types.INTEGER) {
            valSize = Common.getIntSize();
        }
        if (type == Types.DOUBLE) {
            valSize = Common.getDoubleSize();
        }
        if (type == Types.FLOAT) {
            valSize = Common.getDoubleSize();
        }
        if (type == Types.LONG) {
            valSize = Common.getDoubleSize();
        }
        if (type == Types.CHAR) {
            valSize = Common.getCharSize();
        }
        return valSize;
    }

    /**
     * Get bytes of filed value.
     *
     * @param fieldValue
     * @param valSize
     * @return
     */
    private List getBytesOfFiled(String fieldValue, int valSize) {
        
        try {
            List list = new ArrayList();
            byte[] realFieldVal = fieldValue.getBytes("UTF-8");

            // NOTE:If data is too long,cut and write it.
            if (realFieldVal.length > valSize) {
                byte[] partFieldVal = new byte[valSize];
                System.arraycopy(realFieldVal, 0, partFieldVal, 0, valSize);
                realFieldVal = partFieldVal;
            }

            // Write values;
//            byte[] lenBytes = Convertor.int2byte(realFieldVal.length, Common.getDefaultSize());
//            for (int k = 0; k < Common.getDefaultSize(); k++) {
//                list.add(lenBytes[k]);
//            }
            // Write ture values.
            for (int m = 0; m < realFieldVal.length; m++) {
                list.add(realFieldVal[m]);
            }
            for (int k = 0; k < valSize - realFieldVal.length; k++) {

                // Align up 0
                list.add(0);
            }
            return list;
        } catch (Exception e) {
            logger.error(e);
            return null;
        }
        
    }
}
