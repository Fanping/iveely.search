package iveely.search.backstage;

import iveely.search.store.TermLocation;
import iveely.search.store.TermInverted;
import com.iveely.framework.file.Folder;
import iveely.search.store.TextDatabae;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;
import org.apache.log4j.Logger;

/**
 * Index operator.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-1 10:46:22
 */
public class IndexOperator {

    /**
     * Index for term location.
     */
    private List<TermLocation> indexs;

    /**
     * Terms of all keywords.
     */
    private TreeMap<Integer, DynamicIndex> terms;

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(IndexOperator.class.getName());

    /**
     * The path of the backstage data.
     */
    private String backstageData = "";

    /**
     * The path of service data.
     */
    private String serviceData = "";

    /**
     * Initialization.
     */
    private void init() {
        backstageData = "Text_Data";
        serviceData = "Service_Text_Data";
        indexs = new ArrayList<>();
        terms = new TreeMap<>();
        TextDatabae.init("Text_Data");
    }

    public void clean() {
        if (indexs != null) {
            indexs.clear();
            indexs = null;
        }
        if (terms != null) {
            terms.clear();
            terms = null;
        }
        TextDatabae.clean();
    }

    /**
     * Call entry
     *
     * @param arg
     * @return
     */
    public String invoke(String arg) {
        // 0. Init
        init();

        // 1. Process index.
        processIndex();

        // 2. Write index to disk.
        flushTermLocation();

        // 3. backup data for online use.
        Folder.deleteDirectory(this.serviceData);
        TextDatabae.getInstance().backup(this.serviceData);
        terms.clear();
        terms = null;
        logger.info("Index finished.");
        return "Index finished.";
    }

    /**
     * Process index.
     */
    private void processIndex() {
        int totalCount = TextDatabae.getInstance().getTotalCount(new TermInverted());
        int startIndex = 0;
        int step = 1000;
        TermLocation location = new TermLocation();
        for (int i = 0; i <= totalCount / step; i++) {
            if (i % step == 0) {
                logger.info("current:" + i + "/" + (totalCount / step));
            }
            int endIndex = (startIndex + step) > (totalCount - i * step) ? (totalCount - 1) : (startIndex + step);
            location.setStartPostion(startIndex);
            location.setEndPostion(startIndex + step);
            List<TermInverted> list = TextDatabae.getInstance().getTermInverted(location, step);
            list.stream().forEach((term) -> {
                if (terms.containsKey(term.getTerm())) {
                    DynamicIndex dynamicIndex = terms.get(term.getTerm());
                    dynamicIndex.addTermIndex(term);
                } else {
                    DynamicIndex dynamicIndex = new DynamicIndex();
                    dynamicIndex.addTermIndex(term);
                    terms.put(term.getTerm(), dynamicIndex);
                }
            });
            startIndex += step;
        }
    }

    /**
     * Flush all terms's locations.
     */
    private void flushTermLocation() {
        logger.info("Flush Term Location...");
        if (terms.size() > 0) {
            TextDatabae.getInstance().dropTable(new TermInverted());
            TextDatabae.getInstance().createTable(new TermInverted());
            int startPostion = 0;
            int step = 49;
            List<TermInverted> allTerms = new ArrayList<>();
            for (Map.Entry ent : terms.entrySet()) {
                Integer keyword = (Integer) ent.getKey();
                DynamicIndex dynamicIndex = (DynamicIndex) ent.getValue();
                List<TermInverted> dyInverteds = dynamicIndex.getTermInverteds();
                if (dyInverteds.size() == dynamicIndex.getFullSize()) {
                    allTerms.addAll(dyInverteds);
                    int endPostion = startPostion + 49; // Asure insert would be success.
                    TermLocation location = new TermLocation();
                    location.setStartPostion(startPostion);
                    location.setEndPostion(endPostion);
                    location.setTerm(keyword);
                    indexs.add(location);
                    startPostion = endPostion + 1;
                    if (allTerms.size() > 10000) {
                        TextDatabae.getInstance().addTerms(allTerms);
                        allTerms.clear();
                    }
                }
            }
            if (allTerms.size() > 0) {
                TextDatabae.getInstance().addTerms(allTerms);
            }
        }
        TextDatabae.getInstance().dropTable(new TermLocation());
        TextDatabae.getInstance().createTable(new TermLocation());
        TextDatabae.getInstance().addTermLocations(indexs);
    }
}
