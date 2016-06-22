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
import org.apache.mina.filter.codec.CumulativeProtocolDecoder;
import org.apache.mina.filter.codec.ProtocolDecoderOutput;

/**
 * @author Administrator
 */
public class PacketDecoder extends CumulativeProtocolDecoder {

  @Override
  protected boolean doDecode(IoSession session, IoBuffer in, ProtocolDecoderOutput out) throws Exception {
    if (in.remaining() > 0) {
      byte[] sizeBytes = new byte[4];
      in.mark();
      in.get(sizeBytes);
      int size = Convertor.bytesToInt(sizeBytes);

      if (size > in.remaining()) {
        in.reset();
        return false;

      } else {
        byte[] dataBytes = new byte[size];
        in.get(dataBytes, 0, size);
        out.write(dataBytes);
        if (in.remaining() > 0) {
          return true;
        }
      }
    }
    return false;
  }
}
