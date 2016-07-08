package com.iveely.framework.index;

/**
 * author { liufanping@iveely.com }.
 */
public class StoreField<T> {

  private Type type;
  private String name;
  private T value;

  public StoreField(Type type, String name, T value) {
    this.type = type;
    this.name = name;
    this.value = value;
  }

  public Type getType() {
    return type;
  }

  public String getName() {
    return name;
  }

  public T getValue() {
    return value;
  }

  public enum Type {
    STRING,
    LONG,
    TEXT
  }
}
