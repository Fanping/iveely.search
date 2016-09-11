/**
 * date   : 2016年1月31日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.api;

import com.iveely.brain.environment.Variable;
import com.iveely.brain.mind.Awareness;
import com.iveely.brain.mind.Brain;
import com.iveely.brain.mind.Idio;

/**
 * @author {Iveely Liu}
 */
public class Local {

  /**
   * Whether identification has been started.
   */
  private boolean isStarted;

  /**
   * Last hit pattern.
   */
  private String that;

  public Local() {
    isStarted = false;
    Variable.setLocal();
  }

  /**
   * Local service start.
   */
  public void start() {
    if (!isStarted) {
      synchronized (Local.class) {
        if (!isStarted) {
          Awareness.wake();
          isStarted = true;
        }
      }
    }
  }

  /**
   * Start
   *
   * @param msg the message send to brain.
   */
  public String send(String msg) {
    if (!isStarted) {
      start();
    }
    Idio idio = Brain.getInstance().think(msg, that);
    if (idio == null) {
      return null;
    } else {
      this.that = idio.getThat();
      return idio.getResponse();
    }
  }

}
