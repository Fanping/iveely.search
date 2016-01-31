/**
 * date   : 2016年1月30日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.environment;

import bsh.EvalError;
import bsh.Interpreter;

/**
 * @author {Iveely Liu}
 *
 */
public class Script {

	/**
	 * Dynamic execut javascript
	 * 
	 * @param code
	 * @return
	 */
	public static String eval(String code) {
		Interpreter interpreter = new Interpreter();
		try {
			interpreter.set("string", interpreter.eval(code).toString());
			return (String) interpreter.get("string");
		} catch (EvalError e) {
			e.printStackTrace();
			return null;
		}
	}

}
