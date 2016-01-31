import com.iveely.robot.api.Local;
import com.iveely.robot.mind.Awareness;
import com.iveely.robot.net.AsynServer;
import com.iveely.robot.net.Packet;
import com.iveely.robot.net.SyncClient;
import com.iveely.robot.net.SyncServer;
import com.iveely.robot.net.websocket.SocketServer;
import com.iveely.robot.node.ExampleNode;

import bsh.StringUtil;
import groovy.io.EncodingAwareBufferedWriter;

public class Program {

	public static void main(String[] args) {
		if (args != null && args.length > 1) {
			Awareness.wake();
		} else {
			Local local = new Local();
			local.start();
			System.out.println(local.send("马云身高"));
		}
//		ExampleNode node = new ExampleNode(8001);
//		node.start();
	}
}
