/**
 * date   : 2016年1月30日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.mind;

import com.iveely.brain.environment.Variable;
import com.iveely.framework.net.websocket.SocketServer;
import com.iveely.framework.net.websocket.WSHandler;

/**
 * @author {Iveely Liu}
 */
public class Nerve {

  /**
   * Activation foreign service.
   *
   * @return false means termination of service.
   */
  public boolean active() {
    try {
      SocketServer server = new SocketServer(new EventHandler(), Variable.getServiceOfPort());
      server.start();
    } catch (Exception e) {
      e.printStackTrace();
    }
    return false;

  }

  public class EventHandler implements SocketServer.IHandler {

    /**
     * Callback handler.
     */
    private WSHandler handler;

    /*
     * invoke function
     */
    @Override
    public String invoke(Integer sessionId, WSHandler handler, String data) {
      System.out.println(data);
      this.handler = handler;
      Brain.getInstance().request(sessionId, this, data);
      return null;
    }

    /*
     * close session
     */
    @Override
    public void close(Integer sessionId) {
      // TODO Auto-generated method stub
      Brain.getInstance().release(sessionId);

    }

    /**
     * Response information back.
     * @param answer answer
     */
    public void response(String answer) {
      if (handler != null) {
        handler.send(answer);
      }
    }

  }
}
