package com.iveely.framework.net;

import java.io.IOException;
import java.io.InputStream;
import java.io.RandomAccessFile;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.Observable;

/**
 * Downloader.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 17:47:42
 */
public class Downloader extends Observable implements Runnable {

    /**
     * Download status.
     */
    public final String STATUSES[]
            = {
                "Downloading",
                "Paused",
                "Complete",
                "Cancelled",
                "Error"
            };

    /**
     * In downloading.
     */
    public final static int DOWNLOADING = 0;

    /**
     * Paused.
     */
    public final static int PAUSED = 1;

    /**
     * Completed.
     */
    public final static int COMPLETE = 2;

    /**
     * Cancelled.
     */
    public final static int CANCELLED = 3;

    /**
     * Error.
     */
    public final static int ERROR = 4;

    /**
     * The url to download.
     */
    private final URL url;

    /**
     * Download size.
     */
    private int size;

    /**
     * Downloaded size.
     */
    private int downloaded;

    /**
     * Download status.
     */
    private int status;

    public Downloader(String url) throws MalformedURLException {
        this.url = new URL(url);
        size = -1;
        downloaded = 0;
        status = DOWNLOADING;
    }

    /**
     * Get download url.
     *
     * @return
     */
    public String getUrl() {
        return url.toString();
    }

    /**
     * Get download size.
     *
     * @return
     */
    public int getSize() {
        return size;
    }

    /**
     * Get download progress.
     *
     * @return
     */
    public float getProgress() {
        return ((float) downloaded / size) * 100;
    }

    /**
     * Get download status.
     *
     * @return
     */
    public int getStatus() {
        return status;
    }

    /**
     * Pause download.
     */
    public void pause() {
        status = PAUSED;
        stateChanged();
    }

    /**
     * Resume dwonload.
     */
    public void resume() {
        status = DOWNLOADING;
        stateChanged();
        download();
    }

    /**
     * Cancel download.
     */
    public void cancel() {
        status = CANCELLED;
        stateChanged();
    }

    /**
     * Error download.
     */
    private void error() {
        status = ERROR;
        stateChanged();
    }

    /**
     * Start download.
     */
    private void download() {
        Thread thread = new Thread(this);
        thread.start();
    }

    /**
     * Get file name.
     *
     * @param url
     * @return
     */
    private String getFileName(URL url) {
        String fileName = url.getFile();
        return fileName.substring(fileName.lastIndexOf('/') + 1);
    }

    /**
     * Download start.
     */
    @Override
    public void run() {
        RandomAccessFile file = null;
        InputStream stream = null;

        try {
            HttpURLConnection connection
                    = (HttpURLConnection) url.openConnection();
            connection.setRequestProperty("Range",
                    "bytes=" + downloaded + "-");
            connection.connect();
            if (connection.getResponseCode() / 100 != 2) {
                error();
            }
            int contentLength = connection.getContentLength();
            if (contentLength < 1) {
                error();
            }
            if (size == -1) {
                size = contentLength;
                stateChanged();
            }
            file = new RandomAccessFile(getFileName(url), "rw");
            file.seek(downloaded);

            stream = connection.getInputStream();
            while (status == DOWNLOADING) {
                byte buffer[];
                if (size - downloaded > contentLength) {
                    buffer = new byte[contentLength];
                } else {
                    buffer = new byte[size - downloaded];
                }
                int read = stream.read(buffer);
                if (read == -1) {
                    break;
                }
                file.write(buffer, 0, read);
                downloaded += read;
                stateChanged();
            }
            if (status == DOWNLOADING) {
                status = COMPLETE;
                stateChanged();
            }
        } catch (IOException e) {
            error();
        } finally {
            if (file != null) {
                try {
                    file.close();
                } catch (IOException e) {
                    error();
                }
            }
            if (stream != null) {
                try {
                    stream.close();
                } catch (IOException e) {
                }
            }
        }
    }

    /**
     * Status change.
     */
    private void stateChanged() {
        setChanged();
        notifyObservers();
    }
}
