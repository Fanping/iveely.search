/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.iveely.framework.text;

import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author X1 Carbon
 */
public class Triple {

    public Triple() {
        times = new ArrayList<>();
        locations = new ArrayList<>();
        entities = new ArrayList<>();
    }

    private String entityA;

    /**
     * @return the entityA
     */
    public String getEntityA() {
        return entityA;
    }

    /**
     * @param entityA the entityA to set
     */
    public void setEntityA(String entityA) {
        this.entityA = entityA;
    }

    private String relation;

    /**
     * @return the relation
     */
    public String getRelation() {
        return relation;
    }

    /**
     * @param relation the relation to set
     */
    public void setRelation(String relation) {
        this.relation = relation;
    }

    private String entityB;

    /**
     * @return the entityB
     */
    public String getEntityB() {
        return entityB;
    }

    /**
     * @param entityB the entityB to set
     */
    public void setEntityB(String entityB) {
        this.entityB = entityB;
    }

    public boolean isTimesEmpty() {
        return times.isEmpty();
    }

    public void addTime(String time) {
        times.add(time);
    }

    public void addLocation(String location) {
        locations.add(location);
    }

    public boolean isLocationsEmpty() {
        return locations.isEmpty();
    }

    public void addEntity(String entity) {
        entities.add(entity);
    }

    public boolean isEntitesEmpty() {
        return entities.isEmpty();
    }

    private final List<String> times;

    private final List<String> locations;

    private final List<String> entities;

    public String toSimple() {
        return this.entityA + " " + this.relation + " " + this.entityB;
    }

    @Override
    public String toString() {
        return this.entityA + " " + this.relation + " " + this.entityB + "\nTime:" + String.join("、", times) + " Location:"
                + String.join("、", locations) + " Entity:" + String.join("、", entities);
    }
}
