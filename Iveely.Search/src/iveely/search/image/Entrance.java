package iveely.search.image;

import org.apache.log4j.Logger;

/**
 * Entrance of image search.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-23 11:55:19
 */
public class Entrance {

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(iveely.search.backstage.Entrance.class.getName());

    /**
     * Call entry
     *
     * @param arg
     * @return
     */
    public String invoke(String arg) {
        try {
            // 1. Extrator
            Extractor extractor = new Extractor();
            if (extractor.invoke(arg)) {
                // 2. Indexer
                IndexOperator indexOperator = new IndexOperator();
                indexOperator.invoke(arg);
                indexOperator = null;
            }
            extractor = null;
            System.gc();
        } catch (Exception e) {
            logger.error(e);
        }
        return "";
    }
}
