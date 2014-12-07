package iveely.search.store;

import com.iveely.framework.database.type.ShortString;

/**
 * Knowledge base.
 *
 * @author liufanping@iveely.com
 * @date 2014-12-5 20:37:49
 */
public class KnowledgeBase {

    /**
     * Entity a.
     */
    private ShortString entityA;

    /**
     * @return the entityA
     */
    public String getEntityA() {
        if (entityA != null) {
            return entityA.getValue();
        }
        return "";
    }

    /**
     * @param entityA the entityA to set
     * @return
     */
    public boolean setEntityA(String entityA) {
        try {
            this.entityA = new ShortString(entityA);
            return true;
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * Relation
     */
    private ShortString relation;

    /**
     * @return the relation
     */
    public String getRelation() {
        if (this.relation != null) {
            return this.relation.getValue();
        }
        return "";
    }

    /**
     * @param relation the relation to set
     * @return
     */
    public boolean setRelation(String relation) {
        try {
            this.relation = new ShortString(relation);
            return true;
        } catch (Exception ex) {
            return false;
        }
    }

    /**
     * Entity b.
     */
    private ShortString entityB;

    /**
     * @return the entityB
     */
    public String getEntityB() {
        if (this.entityB != null) {
            return this.entityB.getValue();
        }
        return "";
    }

    /**
     * @param entityB the entityB to set
     * @return
     */
    public boolean setEntityB(String entityB) {
        try {
            this.entityB = new ShortString(entityB);
            return true;
        } catch (Exception ex) {
            return false;
        }
    }

    @Override
    public String toString() {
        return getEntityA() + getEntityB() + getRelation();
    }
}
