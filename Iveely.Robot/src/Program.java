import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.Scanner;

import com.iveely.robot.api.Local;
import com.iveely.robot.environment.Script;
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
			BufferedReader reader = new BufferedReader(new InputStreamReader(System.in));
			try {
				String text = reader.readLine().trim();
				while (!text.equals("exit")) {
					System.out.println(local.send(text));
					text = reader.readLine().trim();
				}
				reader.close();
			} catch (IOException ex) {
				System.err.println(ex);
			}
		}
	}
}
