import com.iveely.robot.api.Local;
import com.iveely.robot.mind.Awareness;
import com.iveely.robot.net.websocket.SocketServer;
import com.iveely.robot.node.SocketHandler;

public class Program {

	public static void main(String[] args) {
		if (args != null && args.length > 1) {
			Awareness.wake();
		} else {
			Local local = new Local();
			local.start();
			System.out.println(local.send("你好啊"));
			System.out.println(local.send("东西掉了"));
            System.out.println(local.send("您好啊"));
		}
	}
}
