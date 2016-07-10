import com.iveely.brain.api.Local;
import com.iveely.brain.mind.Awareness;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;

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
