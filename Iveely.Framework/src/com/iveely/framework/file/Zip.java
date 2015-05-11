package com.iveely.framework.file;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.Charset;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;
import java.util.zip.ZipInputStream;
import java.util.zip.ZipOutputStream;
import org.apache.log4j.Logger;

/**
 * Compress and Uncompress.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-19 13:31:17
 */
public class Zip {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Zip.class.getName());

    /**
     * Compress folder.
     *
     * @param folder folder path.
     * @param zipPath store path.
     * @return
     */
    public static boolean compress(String folder, String zipPath) {
        try {
            File inFile = new File(folder);
            if (inFile.exists()) {
                File outFile = new File(zipPath);
                outFile.deleteOnExit();
                ZipOutputStream zos;
                zos = new ZipOutputStream(new FileOutputStream(zipPath), Charset.forName("UTF-8"));
                zos.setComment("com.iveely.computing.file");
                zipFile(inFile, zos, "");
                zos.close();
                return true;
            }
        } catch (IOException e) {
            logger.error(e);
        }
        return false;
    }

    /**
     * Uncompress
     *
     * @param zipPath
     * @param folder
     * @return
     */
    public static boolean uncompress(String zipPath, String folder) {
        OutputStream os = null;
        InputStream is = null;
        try {
            File file = new File(zipPath);
            if (!file.exists()) {
                return false;
            }
            ZipFile zipFile;
            zipFile = new ZipFile(file);
            try (ZipInputStream zipInputStream = new ZipInputStream(new FileInputStream(file), Charset.forName("UTF-8"))) {
                ZipEntry zipEntry;
                while ((zipEntry = zipInputStream.getNextEntry()) != null) {
                    String fileName = zipEntry.getName();
                    File temp = new File(folder + "/" + fileName);
                    boolean isDirExist = true;
                    if (!temp.getParentFile().exists()) {
                        isDirExist = temp.getParentFile().mkdirs();
                    }
                    if (isDirExist) {
                        os = new FileOutputStream(temp);
                        is = zipFile.getInputStream(zipEntry);
                        int len;
                        while ((len = is.read()) != -1) {
                            os.write(len);
                        }
                        os.close();
                        is.close();
                    }
                }
            }
            zipFile.close();
            return true;
        } catch (IOException e) {
            logger.error(e);
        } finally {
            try {
                if (os != null) {
                    os.close();
                }
                if (is != null) {
                    is.close();
                }
            } catch (IOException e) {
                logger.error(e);
            }

        }
        return false;
    }

    /**
     * Zip file.
     *
     * @param inFile
     * @param zos
     * @param dir
     * @throws IOException
     */
    private static void zipFile(File inFile, ZipOutputStream zos, String dir) throws IOException {
        if (inFile.isDirectory()) {
            File[] files = inFile.listFiles();
            for (File file : files) {
                zipFile(file, zos, dir + "\\" + inFile.getName());
            }
        } else {
            String entryName;
            if (!"".equals(dir)) {
                entryName = dir + "\\" + inFile.getName();
            } else {
                entryName = inFile.getName();
            }
            ZipEntry entry = new ZipEntry(entryName);
            zos.putNextEntry(entry);
            try (InputStream is = new FileInputStream(inFile)) {
                int len;
                while ((len = is.read()) != -1) {
                    zos.write(len);
                }
                is.close();
            } catch (Exception e) {
                logger.error(e);
            }

        }
    }
}
