package com.iveely.framework.Media;

import java.awt.Graphics2D;
import java.awt.Image;
import java.awt.RenderingHints;
import java.awt.geom.AffineTransform;
import java.awt.image.BufferedImage;
import java.awt.image.ColorModel;
import java.awt.image.WritableRaster;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import java.util.Base64;
import javax.imageio.ImageIO;
import org.apache.log4j.Logger;

/**
 * Thumbnail for image.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-22 18:35:31
 */
public class Thumbnail {

    /**
     * Logger.
     */
    private static final Logger logger = Logger.getLogger(Thumbnail.class.getName());

    /**
     * Build thumbnail from local file.
     *
     * @param imagePath
     * @param toWidth
     * @param toHeight
     * @return Image data in base64.
     * @throws java.lang.Exception
     */
    public static String getImageBase64(String imagePath,
            int toWidth, int toHeight) throws Exception {
        BufferedImage srcImage;
        File fromFile = new File(imagePath);
        srcImage = ImageIO.read(fromFile);
        if (toWidth > 0 && toHeight > 0) {
            srcImage = resize(srcImage, toWidth, toHeight, true);
        }
        ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
        ImageIO.write(srcImage, "jpg", outputStream);
        byte[] bytes = outputStream.toByteArray();
        return Base64.getEncoder().encodeToString(bytes);
    }

    /**
     * Build thumbnail from online file.
     *
     * @param url
     * @param toWidth
     * @param toHeight
     * @param filterSmall
     * @return Image data in base64.
     */
    public static String getImageBase64FromNet(String url, int toWidth, int toHeight, boolean filterSmall) {
        try {
            URL curl = new URL(url);
            URLConnection con = curl.openConnection();
            con.setConnectTimeout(10000);
            con.setReadTimeout(10000);
            InputStream in = con.getInputStream();
            BufferedImage image = ImageIO.read(in);
            if (image == null || ((image.getWidth() < 75 || image.getHeight() < 75) && filterSmall)) {
                return "";
            }
            image = resize(image, toWidth, toHeight, true);
            ByteArrayOutputStream outputStream = new ByteArrayOutputStream();
            ImageIO.write(image, "gif", outputStream);
            byte[] bytes = outputStream.toByteArray();
            return Base64.getEncoder().encodeToString(bytes);
        } catch (IOException e) {
            logger.error(e);
        }
        return null;
    }

    /**
     * Resize image.
     *
     * @param source
     * @param targetW
     * @param targetH
     * @param equalProportion
     * @return
     */
    public static BufferedImage resize(BufferedImage source, int targetW, int targetH, boolean equalProportion) {
        int type = source.getType();
        BufferedImage target = null;
        double sx = (double) targetW / source.getWidth();
        double sy = (double) targetH / source.getHeight();
        if (equalProportion) {
            if (sx > sy) {
                sx = sy;
                targetW = (int) (sx * source.getWidth());
            } else {
                sy = sx;
                targetH = (int) (sx * source.getHeight());
            }
        }
        if (type == BufferedImage.TYPE_CUSTOM) {
            ColorModel cm = source.getColorModel();
            WritableRaster raster = cm.createCompatibleWritableRaster(targetW, targetH);
            boolean alphaPremultiplied = cm.isAlphaPremultiplied();
            target = new BufferedImage(cm, raster, alphaPremultiplied, null);
        } else {
            target = new BufferedImage(targetW, targetH, Image.SCALE_SMOOTH);
            Graphics2D g = target.createGraphics();
            g.setRenderingHint(RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_QUALITY);
            g.drawRenderedImage(source, AffineTransform.getScaleInstance(sx, sy));
            g.dispose();
        }
        return target;
    }
}
