/*
 * Copyright 2016 liufanping@iveely.com.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package com.iveely.framework.system.net;

import com.iveely.framework.system.text.Convertor;

import org.apache.mina.core.buffer.IoBuffer;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.filter.codec.ProtocolEncoderAdapter;
import org.apache.mina.filter.codec.ProtocolEncoderOutput;

/**
 * @author Administrator
 */
public class PacketEncoder extends ProtocolEncoderAdapter {

  @Override
  public void encode(IoSession session, Object message,
                     ProtocolEncoderOutput out) throws Exception {
    // TODO Auto-generated method stub
    byte[] dataBytes = (byte[]) message;
    byte[] sizeBytes = Convertor.int2byte(dataBytes.length);
    IoBuffer buffer = IoBuffer.allocate(256);
    buffer.setAutoExpand(true);
    buffer.put(sizeBytes);
    buffer.put(dataBytes);
    buffer.flip();
    out.write(buffer);
    out.flush();
    buffer.free();
  }

}
