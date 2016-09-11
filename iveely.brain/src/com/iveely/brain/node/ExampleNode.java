/**
 * date   : 2016年1月31日 author : Iveely Liu contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.brain.node;

import com.iveely.brain.daiml.SimpleString;
import com.iveely.framework.net.Packet;
import com.iveely.framework.net.Packet.MimeType;
import com.iveely.framework.net.SyncServer;

import java.io.IOException;

/**
 * @author {Iveely Liu}
 */
public class ExampleNode {

  /**
   * Synchronous communication.
   */
  public SyncServer server;

  public ExampleNode(int port) {
    this.server = new SyncServer(new Handler(), port);
  }

  /**
   * Start example node to help brain more clever.
   * @throws IOException io exception
   */
  public void start() throws IOException {
    this.server.start();
  }

  public class Handler implements com.iveely.framework.net.SyncServer.ICallback {

    /*
     * (non-Javadoc)
     *
     * @see
     * com.iveely.robot.net.SyncServer.ICallback#invoke(com.iveely.robot.net
     * .Packet)
     */
    @Override
    public Packet invoke(Packet packet) {
      packet.setMimeType(MimeType.STRING.ordinal());
      packet.setData(new SimpleString("Hello!"));
      packet.setExecuteType(1);
      return packet;
    }
  }
}
