package com.iveely.framework.database;

import com.iveely.framework.database.type.Base64Image;
import com.iveely.framework.database.type.Common;
import com.iveely.framework.database.type.ShortString;
import java.io.BufferedInputStream;
import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.io.UnsupportedEncodingException;
import java.lang.reflect.Field;
import java.nio.channels.FileChannel;
import java.util.ArrayList;
import java.util.List;
import org.apache.log4j.Logger;

/**
 * Object store.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-17 22:00:53
 */
public class IOBase {

    /**
     * Database's name.
     */
    private final String dbName;

    /**
     * The object which be stored.
     */
    private final Object storedObject;

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
     * The size of last record.
     */
    private final Integer lastRecordSize = 4;

    /**
     * Max record size.
     */
    private final Integer maxRecodSize = 10000 * 100;

    /**
     * Logger
     */
    private final Logger logger = Logger.getLogger(IOBase.class.getName());

    public IOBase(String dbName, String tableNameVal, Object obj) {
        this.dbName = dbName;
        storedObject = obj;
        tableName = tableNameVal;

        // 1. Each record's size.
        Field[] fields = obj.getClass().getDeclaredFields();
        recordSize = 0;
        for (Field field : fields) {
            String fieldType = field.getType().getSimpleName();
            int valSize = Common.getSimpleStringSize();
            if (fieldType.equals("String")) {
                valSize = Common.getLongStringSize();
            }
            if (fieldType.equals("ShortString")) {
                valSize = Common.getShortStringSize();
            }
            if (fieldType.equals("Base64Image")) {
                valSize = Common.getBase64StringSize();
            }
            if (fieldType.equals("boolean")) {
                valSize = Common.getBooleanSize();
            }
            if (fieldType.equals("Integer") || fieldType.equals("int")) {
                valSize = Common.getIntSize();
            }
            if (fieldType.equals("Double") || fieldType.equals("double")) {
                valSize = Common.getDoubleSize();
            }
            if (fieldType.equals("Float") || fieldType.equals("float")) {
                valSize = Common.getDoubleSize();
            }
            if (fieldType.equals("Long") || fieldType.equals("long")) {
                valSize = Common.getDoubleSize();
            }
            if (fieldType.equals("char") || fieldType.equals("Character")) {
                valSize = Common.getCharSize();
            }

            // 1.1. Header's length.
            recordSize += Common.getDefaultSize();

            // 1.2. Value's size.
            recordSize += valSize;
        }

        // 2. Header size.
        headerSize = Common.getDefaultSize() * 4 + lastRecordSize + fields.length * Common.getDefaultSize();

        // 3.  Init block.
        currentBlockId = 0;
    }

    /**
     * Write an object to hard disk.
     *
     * @param obj
     * @return
     * @throws FileNotFoundException
     * @throws IOException
     * @throws IllegalArgumentException
     */
    public int write(Object obj) throws FileNotFoundException, IOException, IllegalArgumentException {
        if (obj == null) {
            return -1;
        }
        Object[] objs = new Object[1];
        objs[0] = obj;
        return writeMany(objs);
    }

    /**
     * Write many objects to hard disk.
     *
     * @param objs
     * @return
     */
    public int writeMany(Object[] objs) {
        if (objs == null) {
            return -1;
        }
        String rootFile = tableName + ".data";
        if (dbName.length() > 0) {
            rootFile = dbName + "/" + rootFile;
        }

        // 0. Current count.
        int blockFlag = currentBlockId == 0 ? 1 : currentBlockId;
        String fileName = rootFile + "." + blockFlag;
        if (currentBlockId == 0) {
            currentBlockId = getFileBlockCount(fileName);
            fileName = rootFile + "." + currentBlockId;
        }
        int count = getTotalCount(fileName) + 1;

        // 1. Current data file left capacity.
        int capacity = maxRecodSize - ((count < 0 ? 0 : count)) % maxRecodSize;

        // 2. If not need create new data file.
        if (objs.length <= capacity) {
            return writeObjects(objs);
        } else {

            // 3. Create new data file.
            int currentIndex = -1;
            Object[] currentObjs = new Object[capacity];
            for (int i = 0; i < capacity; i++) {
                currentObjs[i] = objs[i];
                currentIndex++;
            }
            if (capacity > 0) {
                writeObjects(currentObjs);
            }
            int lastFlag = -1;
            int size = (objs.length - capacity) / maxRecodSize;
            for (int j = 0; j < size; j++) {
                currentObjs = new Object[maxRecodSize];
                for (int i = 0; i < maxRecodSize; i++) {
                    currentObjs[i] = objs[capacity + j * maxRecodSize + i];
                    currentIndex++;
                }
                lastFlag = writeObjects(currentObjs);
            }
            currentIndex++;
            int leftCapacity = objs.length - currentIndex;
            if (leftCapacity > 0) {
                currentObjs = new Object[leftCapacity];
                for (int i = 0; i < leftCapacity; i++) {
                    currentObjs[i] = objs[currentIndex + i];
                }
                return writeObjects(currentObjs);
            } else {
                return lastFlag;
            }
        }
    }

    /**
     * Read an object from hard disk.
     *
     * @param index
     * @return
     */
    public Object read(int index) {

        // 1. Get file name.
        String rootFile = tableName + ".data";
        if (dbName.length() > 0) {
            rootFile = dbName + "/" + rootFile;
        }

        // 2. Get current record size.
        int blockFlag = currentBlockId == 0 ? 1 : currentBlockId;
        String fileName = rootFile + "." + ((index) / maxRecodSize + 1);
        File dbFile = new File(fileName);
        if (!dbFile.exists()) {
            logger.warn(fileName + " not found.");
            return null;
        }
        index = index % maxRecodSize;

        // 3. Calculate skip size.
        int skipSize = headerSize + recordSize * index;

        // 4. Read data.
        try {
            Object buildObject;
            try (DataInputStream in = new DataInputStream(
                    new BufferedInputStream(
                            new FileInputStream(fileName)))) {
                        long readySkipSize = in.skip(skipSize);
                        while (readySkipSize < skipSize) {
                            readySkipSize += in.skip(skipSize - readySkipSize);
                        }
                        buildObject = storedObject;
                        Field[] fields = buildObject.getClass().getDeclaredFields();
                        for (Field field : fields) {
                            field.setAccessible(true);
                            int valSize = 0;
                            byte[] lengthOfData = new byte[Common.getDefaultSize()];
                            int readySize = in.read(lengthOfData);
                            while (readySize < Common.getDefaultSize()) {
                                logger.warn("expect size:" + Common.getDefaultSize() + ",actual size" + readySize);
                                readySize += in.read(lengthOfData, readySize, Common.getDefaultSize() - readySize);
                            }
                            valSize = Convertor.bytesToInt(lengthOfData);
                            byte[] bytes = new byte[valSize];
                            readySize = in.read(bytes);
                            String data = new String(bytes, "UTF-8");
                            String fieldType = field.getType().getSimpleName();
                            int memSize = Common.getSimpleStringSize();
                            switch (fieldType) {
                                case "Integer":
                                case "int":
                                    if ("".equals(data)) {
                                        data = "0";
                                    }
                                    field.set(buildObject, Integer.parseInt(data));
                                    memSize = Common.getIntSize();
                                    break;
                                case "Double":
                                case "double":
                                    if ("".equals(data)) {
                                        data = "0";
                                    }
                                    field.set(buildObject, Double.parseDouble(data));
                                    memSize = Common.getDoubleSize();
                                    break;
                                case "Float":
                                case "float":
                                    if ("".equals(data)) {
                                        data = "0";
                                    }
                                    field.set(buildObject, Float.parseFloat(data));
                                    memSize = Common.getDoubleSize();
                                    break;
                                case "Long":
                                case "long":
                                    if ("".equals(data)) {
                                        data = "0";
                                    }
                                    field.set(buildObject, Long.parseLong(data));
                                    memSize = Common.getDoubleSize();
                                    break;
                                case "String":
                                    field.set(buildObject, data);
                                    memSize = Common.getLongStringSize();
                                    break;
                                case "ShortString":
                                    field.set(buildObject, new ShortString(data));
                                    memSize = Common.getShortStringSize();
                                    break;
                                case "Base64Image":
                                    Base64Image image = new Base64Image();
                                    image.setBase64(data);
                                    field.set(buildObject, image);
                                    memSize = Common.getBase64StringSize();
                                    break;
                                case "boolean":
                                    if (data.equals("1")) {
                                        field.set(buildObject, true);
                                    } else {
                                        field.set(buildObject, false);
                                    }
                                    break;
                                case "Character":
                                case "char":
                                    if ("".equals(data)) {
                                        data = " ";
                                    }
                                    field.set(buildObject, data.toCharArray()[0]);
                                    memSize = Common.getCharSize();
                                    break;
                                default:
                                    logger.error("Not found any type," + fieldType);
                            }
                            if (!fieldType.equals("boolean")) {
                                long skipSteps = in.skip(memSize - valSize);
                                while (skipSteps != memSize - valSize) {
                                    skipSteps += in.skip(memSize - valSize - skipSteps);
                                }
                            }
                        }
                    }
                    return buildObject;
        } catch (Exception e) {
            logger.error(e.getMessage() + "\n index=" + index + " \n fileName=" + fileName);
        } finally {

        }
        return null;
    }

    /**
     * Update data on hard disk.
     *
     * @param index index location.
     * @param obj new object.
     * @return
     */
    public boolean update(int index, Object obj) {

        // 1. Get file name.
        String rootFile = tableName + ".data";
        if (dbName.length() > 0) {
            rootFile = dbName + "\\" + rootFile;
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
            }
            return isUpdateSuccess;
        } catch (IOException e) {
            logger.warn(e);
        } finally {

        }
        return false;
    }

    /**
     * Get count of data.
     *
     * @param obj
     * @return
     */
    public int getCount(Object obj) {
        if (obj == null) {
            return -1;
        }
        String rootFile = tableName + ".data";
        if (dbName.length() > 0) {
            rootFile = dbName + "/" + rootFile;
        }
        int blockFlag = currentBlockId == 0 ? 1 : currentBlockId;
        String fileName = rootFile + "." + blockFlag;
        if (currentBlockId == 0) {
            currentBlockId = getFileBlockCount(fileName);
            fileName = rootFile + "." + currentBlockId;
        }
        int count = getTotalCount(rootFile + "." + currentBlockId) + 1;
        return count;
    }

    /**
     * Get total count of the data store.
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
            buffer = null;
            channel.close();
        }
        channel = null;
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
            for (int i = 1; i < count; i++) {
                String fileNameString = rootFile + "." + i;
                byte[] countBytes = Convertor.int2byte(count, Common.getDefaultSize());
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
    private byte[] getObjBytes(Object obj) {
        if (obj == null) {
            return null;
        }
        List list = new ArrayList();
        int totalSize = 0;
        try {
            Field[] fields = obj.getClass().getDeclaredFields();
            for (Field field : fields) {
                field.setAccessible(true);
                String fieldType = field.getType().getSimpleName();
                int valSize = Common.getSimpleStringSize();
                Object objVal = field.get(obj);
                Object defaultVal = new Object();
                if (fieldType.equals("String")) {
                    valSize = Common.getLongStringSize();
                    defaultVal = "";
                }
                if (fieldType.equals("ShortString")) {
                    valSize = Common.getShortStringSize();
                    defaultVal = ShortString.getDefaultShortString();
                }
                if (fieldType.equals("Base64Image")) {
                    valSize = Common.getBase64StringSize();
                    defaultVal = new Base64Image();
                }
                if (fieldType.equals("boolean")) {
                    valSize = Common.getBooleanSize();
                    defaultVal = false;

                }
                if (fieldType.equals("Integer") || fieldType.equals("int")) {
                    valSize = Common.getIntSize();
                    defaultVal = 0;
                }
                if (fieldType.equals("Double") || fieldType.equals("double")) {
                    valSize = Common.getDoubleSize();
                    defaultVal = 0;
                }
                if (fieldType.equals("Float") || fieldType.equals("float")) {
                    valSize = Common.getDoubleSize();
                    defaultVal = 0;
                }
                if (fieldType.equals("Long") || fieldType.equals("long")) {
                    valSize = Common.getDoubleSize();
                    defaultVal = 0;
                }
                if (fieldType.equals("char") || fieldType.equals("Character")) {
                    valSize = Common.getCharSize();
                    defaultVal = ' ';
                }

                byte[] fieldValBytes = new byte[valSize + Common.getDefaultSize()];
                if (objVal == null) {
                    objVal = defaultVal;
                }
                String fieldValue = objVal.toString();
                if (fieldType.equals("boolean")) {
                    if (fieldValue.equals("true")) {
                        fieldValue = "1";
                    } else {
                        fieldValue = "0";
                    }
                }
                byte[] realFieldVal = fieldValue.getBytes("UTF-8");

                // NOTE:If data is too long,cut and store it.
                if (realFieldVal.length > valSize) {
                    byte[] partFieldVal = new byte[valSize];
                    System.arraycopy(realFieldVal, 0, partFieldVal, 0, valSize);
                    realFieldVal = partFieldVal;
                }

                // Write values;
                byte[] lenBytes = Convertor.int2byte(realFieldVal.length, Common.getDefaultSize());
                for (int k = 0; k < Common.getDefaultSize(); k++) {
                    list.add(lenBytes[k]);
                }

                // Write ture values.
                for (int m = 0; m < realFieldVal.length; m++) {
                    list.add(realFieldVal[m]);
                }
                for (int k = 0; k < valSize - realFieldVal.length; k++) {

                    // Align up 0
                    list.add(0);
                }
            }
        } catch (UnsupportedEncodingException | IllegalAccessException | IllegalArgumentException | SecurityException e) {
            logger.error(e);
            return null;
        }
        byte[] bytes = new byte[list.size()];
        for (int i = 0; i < bytes.length; i++) {
            bytes[i] = Byte.parseByte(list.get(i).toString());
        }
        return bytes;
    }

    /**
     * Write many objects.
     *
     * @param objs
     * @return
     */
    private int writeObjects(Object[] objs) {
        if (objs == null) {
            return -1;
        }
        try {
            String rootFile = tableName + ".data";
            if (dbName.length() > 0) {
                rootFile = dbName + "/" + rootFile;
            }

            // 1. Get count of the data.
            int blockFlag = currentBlockId == 0 ? 1 : currentBlockId;
            String fileName = rootFile + "." + blockFlag;
            if (currentBlockId == 0) {
                currentBlockId = getFileBlockCount(fileName);
                fileName = rootFile + "." + currentBlockId;
            }
            int count = getTotalCount(rootFile + "." + currentBlockId);

            // 2. If first write data.
            if ((count + 1) % maxRecodSize == 0) {
                currentBlockId = (count + 1) / maxRecodSize + 1;
                fileName = rootFile + "." + currentBlockId;

                //---------------------------------------------------------------------------------------------------------
                // 4byte:version| 4byte:当前文件块数 | 4byte：表记录数 | 4byte:Header长度 | 4byte:表标识 | cloumntype(4byte) | 
                //--------------------------------------------------------------------------------------------------------
                // 2.1 Write data.
                try (DataOutputStream out = new DataOutputStream(new FileOutputStream(fileName))) {

                    // 2.1.1 Write current version of data.
                    byte[] version = Convertor.int2byte(1, Common.getDefaultSize());
                    out.write(version);

                    // 2.1.2 Write file block count.
                    byte[] blocks = Convertor.int2byte(currentBlockId, Common.getDefaultSize());
                    out.write(blocks);

                    // 2.1.3 Write table's record area.
                    byte[] tabLength = Convertor.int2byte(0, Common.getDefaultSize());
                    out.write(tabLength);

                    // 2.1.4 Write header's length.
                    byte[] headerSizeBytes = Convertor.int2byte(headerSize, Common.getDefaultSize());
                    out.write(headerSizeBytes);

                    // 2.1.5 Write table's name.
                    String tabName = tableName;
                    byte[] realBytes = Convertor.int2byte(tabName.hashCode(), Common.getDefaultSize());
                    out.write(realBytes);

                    // 2.1.6 Write clumn's type.
                    Field[] fields = objs[0].getClass().getDeclaredFields();
                    for (Field field : fields) {
                        String fieldType = field.getType().getSimpleName();
                        byte[] bytes = getHeaderType(fieldType);
                        out.write(bytes);
                    }
                }
                notifyAddBlock(rootFile, currentBlockId);
            }

            // 3. Write data.
            boolean isWriteSuccess = false;
            try (DataOutputStream dbAppender = new DataOutputStream(new FileOutputStream(fileName, true))) {
                List<byte[]> list = new ArrayList();
                byte[] singleObj = getObjBytes(objs[0]);
                if (singleObj != null) {
                    list.add(singleObj);
                }
                for (int m = 1; m < objs.length; m++) {
                    singleObj = getObjBytes(objs[m]);
                    if (singleObj != null) {
                        list.add(singleObj);
                    }
                }
                if (objs.length > 1 && singleObj != null) {
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
                byte[] currentCount = Convertor.int2byte(count + objs.length, Common.getDefaultSize());
                modifyFile(fileName, Common.getDefaultSize() * 2, currentCount);
                return count + objs.length;
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
                headerType = HeaderType.INTEGER.ordinal();
                break;
            case "double":
                headerType = HeaderType.DOUBLE.ordinal();
                break;
            case "float":
                headerType = HeaderType.FLOAT.ordinal();
                break;
            case "long":
                headerType = HeaderType.LONG.ordinal();
                break;
            case "char":
            case "character":
                headerType = HeaderType.CHAR.ordinal();
                break;
            case "string":
                headerType = HeaderType.STRING.ordinal();
                break;
            case "shortstring":
                headerType = HeaderType.SHORTSTRING.ordinal();
                break;
            case "boolean":
            case "bool":
                headerType = HeaderType.BOOLEAN.ordinal();
                break;
            case "base64image":
                headerType = HeaderType.IMAGE.ordinal();
                break;
            default:
                logger.error("Not found any type," + typeString);
        }
        return Convertor.int2byte(headerType, Common.getTypeSize());
    }
}
