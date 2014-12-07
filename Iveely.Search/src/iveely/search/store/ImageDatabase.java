package iveely.search.store;

import com.iveely.framework.database.Engine;
import com.iveely.framework.file.Folder;
import java.io.File;
import java.util.ArrayList;
import java.util.List;

/**
 * Pictures operational database.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-23 12:05:00
 */
public class ImageDatabase {

    /**
     * Data storage engine.
     */
    private Engine engine;

    /**
     * Single instance.
     */
    private static ImageDatabase database;

    /**
     * Database name.
     */
    private static String dbName;

    private ImageDatabase(boolean force) {
        if (engine == null || force) {
            engine = new Engine();
            engine.createDatabase(dbName);
            engine.createTable(new Url());
            engine.createTable(new Status());
            engine.createTable(new TermInverted());
            engine.createTable(new TermLocation());
            engine.createTable(new Image());
            engine.write(new Status());
        }
    }

    /**
     * Initialization.
     *
     * @param name
     */
    public static void init(String name) {
        dbName = name;
    }

    /**
     * Rename databse.
     *
     * @param name
     */
    public static void rename(String name) {
        File file = new File(dbName);
        if (file.exists()) {
            file.renameTo(new File(name));
        }
        dbName = name;
        database = new ImageDatabase(true);
    }

    /**
     * Drop all tables.
     */
    public static void drop() {
        Folder.deleteDirectory(dbName);
    }

    /**
     * Drop table.
     *
     * @param obj
     */
    public void dropTable(Object obj) {
        File file = new File(dbName);
        if (file.exists() && file.isDirectory()) {
            //TODO:Maybe misunderstanding when delete some files.
            String startName = obj.getClass().getSimpleName();
            String[] childFiles = file.list();
            for (String child : childFiles) {
                if (child.startsWith(startName)) {
                    File childFile = new File(dbName + "/" + child);
                    childFile.delete();
                }
            }
        }
    }

    /**
     * Create table.
     *
     * @param obj
     * @return
     */
    public boolean createTable(Object obj) {
        return engine.createTable(obj);
    }

    /**
     * Get DB name.
     *
     * @return
     */
    public String getDbName() {
        return dbName;
    }

    /**
     * Get instance.
     *
     * @return
     */
    public static ImageDatabase getInstance() {
        if (database == null) {
            database = new ImageDatabase(false);
        }
        return database;
    }

    /**
     * Add images.
     *
     * @param list
     * @return
     */
    public int addImages(List<Image> list) {
        Image[] images = new Image[list.size()];
        images = list.toArray(images);
        return engine.writeMany(images);
    }

    /**
     * Get images.
     *
     * @param start
     * @param end
     * @return
     */
    public List<Image> getImages(int start, int end) {
        List<Image> images = new ArrayList<>();
        for (int i = start; i < end + 1; i++) {
            Image temp = (Image) engine.read(new Image(), i);
            Image image = new Image();
            image.setAlt(temp.getAlt());
            image.setUrl(temp.getUrl());
            image.setIsLogo(temp.isIsLogo());
            image.setImage(temp.getImage());
            image.setId(temp.getId());
            images.add(image);
        }
        return images;
    }

    /**
     * Get image by index number.
     *
     * @param index
     * @return
     */
    public Image getImage(int index) {
        if (index < 0) {
            return null;
        }
        Object object = engine.read(new Image(), index);
        if (object == null) {
            return null;
        }
        Image temp = (Image) object;
        Image image = new Image();
        image.setAlt(temp.getAlt());
        image.setUrl(temp.getUrl());
        image.setIsLogo(temp.isIsLogo());
        image.setImage(temp.getImage());
        image.setId(temp.getId());
        return temp;
    }

    /**
     * Get url.
     *
     * @param id
     * @return
     */
    public Url getUrl(int id) {
        int count = engine.getTotalCount(new Url());
        if (count < id) {
            return null;
        }
        return (Url) engine.read(new Url(), id);
    }

    /**
     * Add index.
     *
     * @param indexs
     * @return
     */
    public int addIndex(Object[] indexs) {
        return engine.writeMany(indexs);
    }

    /**
     * Get text search term invert.
     *
     * @param obj
     * @param count
     * @return
     */
    public List<TermInverted> getTermInverted(TermLocation obj, int count) {
        List<TermInverted> list = new ArrayList<>();
        for (int i = obj.getStartPostion(); i < obj.getEndPostion() + 1 && count > 0; i++) {
            TermInverted termInverted = (TermInverted) engine.read(new TermInverted(), i);
            if (termInverted != null) {
                TermInverted fInverted = new TermInverted();
                fInverted.setPage(termInverted.getPage());
                fInverted.setRank(termInverted.getRank());
                fInverted.setTerm(termInverted.getTerm());
                list.add(fInverted);
                count--;
            }
        }
        return list;
    }

    /**
     * Get status.
     *
     * @return
     */
    public Status getStatus() {
        Object status = engine.read(new Status(), 0);
        if (status == null) {
            return null;
        } else {
            return (Status) status;
        }
    }

    /**
     * Update status.
     *
     * @param status
     */
    public void updateStatus(Status status) {
        engine.update(0, status);
    }

    /**
     * Update url.
     *
     * @param id
     * @param url
     * @return
     */
    public boolean updateUrl(int id, Url url) {
        return engine.update(id, url);
    }

    /**
     * Bulk add urls.
     *
     * @param urls
     */
    public void addUrls(Url[] urls) {
        engine.writeMany(urls);
    }

    public int addUrl(Url url) {
        return engine.write(url);
    }

    /**
     * Backup database.
     *
     * @param name
     */
    public void backup(String name) {
        engine.backup(name);
    }

    /**
     * Get all visited URL.
     *
     * @return
     */
    public List<Url> getUrls() {
        int count = engine.getTotalCount(new Url());
        List<Url> urls = new ArrayList<>();
        for (int i = 0; i < count; i++) {
            Url url = (Url) engine.read(new Url(), i);
            if (url != null) {
                urls.add(url);
            }
        }
        return urls;
    }

    /**
     * Add inverted index set.
     *
     * @param terms
     * @return
     */
    public int addTerms(List<TermInverted> terms) {
        TermInverted[] termArray = new TermInverted[terms.size()];
        termArray = terms.toArray(termArray);
        return engine.writeMany(termArray);
    }

    /**
     * Add term location index.
     *
     * @param locations
     */
    public void addTermLocations(List<TermLocation> locations) {
        TermLocation[] locationArray = new TermLocation[locations.size()];
        locationArray = locations.toArray(locationArray);
        engine.writeMany(locationArray);
    }

    /**
     * Add term location index.
     *
     * @param location
     */
    public void addTermLocation(TermLocation location) {
        engine.write(location);
    }

    /**
     * Get all term locations.
     *
     * @return
     */
    public List<TermLocation> getTermLocations() {
        int count = engine.getTotalCount(new TermLocation());
        List<TermLocation> locations = new ArrayList<>();
        for (int i = 0; i < count; i++) {
            TermLocation location = (TermLocation) engine.read(new TermLocation(), i);
            TermLocation flocal = new TermLocation();
            flocal.setEndPostion(location.getEndPostion());
            flocal.setStartPostion(location.getStartPostion());
            flocal.setTerm(location.getTerm());
            locations.add(flocal);
        }
        return locations;
    }

    /**
     * Get total count.
     *
     * @param obj
     * @return
     */
    public int getTotalCount(Object obj) {
        return engine.getTotalCount(obj);
    }
}
