package com.iveely.plugins.pagesearch.data;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.TreeSet;

/**
 * DynamicIndex.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-1 12:47:03
 */
public class DynamicIndex implements Serializable {

    /**
     * The invert terms.
     */
    private TreeSet<SpeciObj> terms;

    /**
     * The Max size of the terms.
     */
    private final Integer maxSize = 50;

    public DynamicIndex() {
        terms = new TreeSet<>();
        for (int i = 0; i < maxSize; i++) {
            TermInverted term = new TermInverted();
            term.setPage(-1);
            term.setRank(-1);
            term.setTerm(-1);
            SpeciObj obj = new SpeciObj();
            obj.setObj(term);
            obj.setRank(-1);
            terms.add(obj);
        }
    }

    /**
     * Add term invert.
     *
     * @param term
     */
    public void addTermIndex(TermInverted term) {
        if (term != null) {
            SpeciObj obj = new SpeciObj();
            obj.setObj(term);
            obj.setRank(term.getRank());
            terms.add(obj);
            terms.remove(terms.last());
        }
    }

    /**
     * Get index full size.
     *
     * @return
     */
    public int getFullSize() {
        return maxSize;
    }

    /**
     * Get all term inverted.
     *
     * @return
     */
    public List<TermInverted> getTermInverteds() {
        List<TermInverted> list = new ArrayList<>();
        Iterator<SpeciObj> iterator = terms.iterator();
        while (iterator.hasNext()) {
            SpeciObj obj = (SpeciObj) iterator.next();
            TermInverted termInverted = (TermInverted) obj.getObj();
            list.add(termInverted);
        }
        return list;
    }
}
