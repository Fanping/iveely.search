package iveely.search.backstage;

import com.iveely.framework.segment.DicSegment;
import org.apache.log4j.Logger;

/**
 * Entrance for text search.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 17:13:14
 */
public class Entrance {

    /**
     * Logger.
     */
    private final Logger logger = Logger.getLogger(Entrance.class.getName());

    /**
     * Call entry
     *
     * @param arg
     * @return
     */
    public String invoke(String arg) {
        try {
            // 1. Spider
            Crawler crawler = new Crawler();
            if (crawler.invoke(arg)) {
                // 2. Indexer
                IndexOperator indexOperator = new IndexOperator();
                indexOperator.invoke(arg);
                indexOperator.clean();
                indexOperator = null;
            }
            crawler.clean();
            crawler = null;
            System.gc();
        } catch (Exception e) {
            logger.error(e);
        }
        return "";
    }
}
