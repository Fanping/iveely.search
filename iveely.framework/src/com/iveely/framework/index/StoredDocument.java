package com.iveely.framework.index;

import java.util.List;

/**
 * author { liufanping@iveely.com }.
 */
public class StoredDocument {

  private List<StoreField> storeFields;

  public StoredDocument(List<StoreField> storeFields) {
    this.storeFields = storeFields;
  }

  public List<StoreField> getStoreFields() {
    return storeFields;
  }
}
