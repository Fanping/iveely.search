package com.iveely.plugins.pagesearch.data;

import java.io.Serializable;

/*
 Special objects
 */
public class SpeciObj implements Comparable, Serializable {

    /**
     * Target weights.
     */
    private double rank;

    public double getRank() {
        return rank;
    }

    public void setRank(double rankVal) {
        this.rank = rankVal;
    }

    /**
     * Object.
     */
    private Object obj;

    public Object getObj() {
        return this.obj;
    }

    public void setObj(Object objVal) {
        this.obj = objVal;
    }

    @Override
    public int compareTo(Object o) {
        SpeciObj otherObj = (SpeciObj) o;
        if (otherObj.getObj().equals(obj)) {
            return 0;
        }
        if (otherObj.getRank() < getRank()) {
            return -1;
        }
        return 1;
    }
}
