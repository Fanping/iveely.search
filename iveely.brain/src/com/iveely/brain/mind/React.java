/**
 * date   : 2016年1月29日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.mind;

/**
 * @author {Iveely Liu}
 */
public class React {

  /**
   * Status of current react.
   */
  private Status status;
  /**
   * Result data.
   */
  private String ret;
  /**
   * Hit pattern.
   */
  private String that;

  public React(Status status) {
    this.status = status;
  }

  /**
   * @return  the status
   */
  public Status getStatus() {
    return this.status;
  }

  /**
   * @return the ret
   */
  public String getRet() {
    return ret;
  }

  /**
   * @param ret the ret to set
   */
  public void setRet(String ret) {
    this.ret = ret;
  }

  /**
   * Get that.
   */
  public String getThat() {
    return this.that;
  }

  /**
   * Set hit pattern.
   * @param text text
   */
  public void setThat(String text) {
    this.that = text;
  }

  /**
   * The status for match pattern.
   *
   * @author {Iveely Liu}
   */
  public enum Status {
    // The final result.
    SUCCESS,
    // Can not found the result.
    FAILURE,
    // Continue to find answer.
    RECURSIVE
  }
}
