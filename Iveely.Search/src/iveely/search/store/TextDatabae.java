package iveely.search.store;

import com.iveely.framework.database.Engine;
import com.iveely.framework.file.Folder;
import java.io.File;
import java.util.ArrayList;
import java.util.List;

/**
 * Text database.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-21 21:59:16
 */
public class TextDatabae {

    /**
     * Data storage engine.
     */
    private Engine engine;

    /**
     * Single instance.
     */
    private static TextDatabae database;

    /**
     * Database's name.
     */
    private static String dbName;

    private TextDatabae(boolean force) {
        if (engine == null || force) {
            engine = new Engine();
            engine.createDatabase(dbName);
            engine.createTable(new HtmlPage());
            engine.createTable(new Url());
            engine.createTable(new Status());
            engine.createTable(new TermInverted());
            engine.createTable(new TermLocation());
            engine.createTable(new Wikipedia());
            engine.createTable(new WikipediaLocation());
            engine.createTable(new KnowledgeBase());
            engine.createTable(new KnowledgeIndex());
            engine.createTable(new KTermLocation());
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
     * Rename of databse.
     *
     * @param name
     */
    public static void rename(String name) {
        File file = new File(dbName);
        if (file.exists()) {
            file.renameTo(new File(name));
        }
        dbName = name;
        database = new TextDatabae(true);
    }

    /**
     * Drop all tables.
     */
    public static void drop() {
        Folder.deleteDirectory(dbName);
    }

    /**
     * Drop a table.
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
                    boolean isdeleted = childFile.delete();
                    if (!isdeleted) {
                        System.gc();
                        isdeleted = childFile.delete();
                    }
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
     * Get name of database.
     *
     * @return
     */
    public String getDbName() {
        return dbName;
    }

    /**
     * Get single instance.
     *
     * @return
     */
    public static TextDatabae getInstance() {
        if (database == null) {
            database = new TextDatabae(false);
        }
        return database;
    }

    /**
     * Clean database,database would be null,but data is here.
     */
    public static void clean() {
        if (database != null) {
            database.engine = null;
            database = null;
        }
    }

    /**
     * Add page.
     *
     * @param page
     * @return
     */
    public int addPage(HtmlPage page) {
        return engine.write(page);
    }

    /**
     * Get page by id.
     *
     * @param id
     * @return
     */
    public HtmlPage getPage(int id) {
        return (HtmlPage) engine.read(new HtmlPage(), id);
    }

    /**
     * Update page by id.
     *
     * @param id
     * @param page
     * @return
     */
    public boolean updatePage(int id, HtmlPage page) {
        return engine.update(id, page);
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
     * Bulk insert urls.
     *
     * @param urls
     */
    public void addUrls(Url[] urls) {
        engine.writeMany(urls);
    }

    /**
     * Insert an url.
     *
     * @param url
     * @return
     */
    public int addUrl(Url url) {
        return engine.write(url);
    }

    /**
     * Get url by id.
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
     * Add many indexs.
     *
     * @param indexs
     * @return
     */
    public int addIndex(Object[] indexs) {
        return engine.writeMany(indexs);
    }

    /**
     * Get inverted index.
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
     * Backup data.
     *
     * @param name
     */
    public void backup(String name) {
        engine.backup(name);
    }

    /**
     * Get all visited urls.
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
     * Add index of term locations.
     *
     * @param locations
     */
    public void addTermLocations(List<TermLocation> locations) {
        TermLocation[] locationArray = new TermLocation[locations.size()];
        locationArray = locations.toArray(locationArray);
        engine.writeMany(locationArray);
    }

    /**
     * Get wiki by id.
     *
     * @param id
     * @return
     */
    public Wikipedia getWiki(int id) {
        Wikipedia wiki = (Wikipedia) engine.read(new Wikipedia(), id);
        return wiki;
    }

    /**
     * Add term location.
     *
     * @param location
     */
    public void addTermLocation(TermLocation location) {
        engine.write(location);
    }

    /**
     * Get all inverted index.
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
     * Get all wikis' locations.
     *
     * @return
     */
    public List<WikipediaLocation> getWikipediaLocations() {
        int count = engine.getTotalCount(new WikipediaLocation());
        List<WikipediaLocation> locations = new ArrayList<>();
        for (int i = 0; i < count; i++) {
            WikipediaLocation location = (WikipediaLocation) engine.read(new WikipediaLocation(), i);
            WikipediaLocation flocal = new WikipediaLocation();
            flocal.setStoreId(location.getStoreId());
            flocal.setTerm(location.getTerm());
            locations.add(flocal);
        }
        return locations;
    }

    /**
     * Add many wikis.
     *
     * @param wikis
     */
    public void addWikis(List<Wikipedia> wikis) {
        if (wikis.size() > 0) {
            Wikipedia[] wikipedias = new Wikipedia[wikis.size()];
            wikipedias = wikis.toArray(wikipedias);
            engine.writeMany(wikipedias);
        }
    }

    /**
     * Add many wikipedia locations.
     *
     * @param locations
     */
    public void addLocations(List<WikipediaLocation> locations) {
        if (locations.size() > 0) {
            WikipediaLocation[] wikipedias = new WikipediaLocation[locations.size()];
            wikipedias = locations.toArray(wikipedias);
            engine.writeMany(wikipedias);
        }
    }

    /**
     * Get wikipedias.
     *
     * @param from
     * @param to
     * @return
     */
    public List<Wikipedia> getWikipedias(int from, int to) {
        List<Wikipedia> wikis = new ArrayList<>();
        for (int i = from; i <= to; i++) {
            Wikipedia wiki = (Wikipedia) engine.read(new Wikipedia(), i);
            Wikipedia fwiki = new Wikipedia();
            fwiki.setAbsArticle(wiki.getAbsArticle());
            fwiki.setTheme(wiki.getTheme());
            wikis.add(fwiki);
        }
        return wikis;
    }

    /**
     * Add many knowledge base.
     *
     * @param bases
     */
    public void addKnowledgeBase(List<KnowledgeBase> bases) {
        if (bases.size() > 0) {
            KnowledgeBase[] knowledgeBases = new KnowledgeBase[bases.size()];
            knowledgeBases = bases.toArray(knowledgeBases);
            engine.writeMany(knowledgeBases);
        }
    }

    /**
     * Get knowledge base.
     *
     * @param from
     * @param to
     * @return
     */
    public List<KnowledgeBase> getKnowledgeBase(int from, int to) {
        List<KnowledgeBase> bases = new ArrayList<>();
        for (int i = from; i < to; i++) {
            KnowledgeBase base = (KnowledgeBase) engine.read(new KnowledgeBase(), i);
            KnowledgeBase fbase = new KnowledgeBase();
            fbase.setEntityA(base.getEntityA());
            fbase.setEntityB(base.getEntityB());
            fbase.setRelation(base.getRelation());
            bases.add(fbase);
        }
        return bases;
    }

    /**
     * Add many knowledge indexs.
     *
     * @param indexs
     */
    public void addKnowledgeIndex(List<KnowledgeIndex> indexs) {
        if (indexs.size() > 0) {
            KnowledgeIndex[] knowledgeIndexs = new KnowledgeIndex[indexs.size()];
            knowledgeIndexs = indexs.toArray(knowledgeIndexs);
            engine.writeMany(knowledgeIndexs);
        }
    }

    /**
     * Get knowledge indexs.
     *
     * @param from
     * @param to
     * @param count
     * @return
     */
    public List<KnowledgeIndex> getKnowledgeIndexs(int from, int to, int count) {
        List<KnowledgeIndex> indexs = new ArrayList<>();
        for (int i = from; i < to && i < from + count; i++) {
            KnowledgeIndex index = (KnowledgeIndex) engine.read(new KnowledgeIndex(), i);
            if (index != null && index.getKnowledgeId() != -1) {
                KnowledgeIndex findex = new KnowledgeIndex();
                findex.setKnowledgeId(index.getKnowledgeId());
                findex.setLocation(index.getLocation());
                findex.setRank(index.getRank());
                findex.setTerm(index.getTerm());
                indexs.add(findex);
            }
        }
        return indexs;
    }

    /**
     * Add many knownledge term locations.
     *
     * @param locations
     */
    public void addKTLocation(List<KTermLocation> locations) {
        if (locations.size() > 0) {
            KTermLocation[] kTermLocations = new KTermLocation[locations.size()];
            kTermLocations = locations.toArray(kTermLocations);
            engine.writeMany(kTermLocations);
        }
    }

    /**
     * Get knownledge term locations.
     *
     * @return
     */
    public List<KTermLocation> getKTermLocations() {
        int count = getTotalCount(new KTermLocation());
        List<KTermLocation> locations = new ArrayList<>();
        for (int i = 0; i < count; i++) {
            KTermLocation location = (KTermLocation) engine.read(new KTermLocation(), i);
            if (location != null) {
                KTermLocation fLocation = new KTermLocation();
                fLocation.setEndPostion(location.getEndPostion());
                fLocation.setStartPostion(location.getStartPostion());
                fLocation.setTerm(location.getTerm());
                locations.add(fLocation);
            }
        }
        return locations;
    }

    /**
     * Get knowledge base by id.
     *
     * @param id
     * @return
     */
    public KnowledgeBase getKnowledgeBase(int id) {
        return (KnowledgeBase) engine.read(new KnowledgeBase(), id);
    }

    /**
     * Get total count for an object.
     *
     * @param obj
     * @return
     */
    public int getTotalCount(Object obj) {
        return engine.getTotalCount(obj);
    }
}
