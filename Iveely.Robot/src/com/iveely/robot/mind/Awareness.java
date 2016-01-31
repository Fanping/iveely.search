/**
 * date   : 2016年1月28日
 * author : Iveely Liu
 * contact: sea11510@mail.ustc.edu.cn
 */
package com.iveely.robot.mind;

import com.iveely.robot.daiml.CategoryLoader;
import com.iveely.robot.daiml.SetLoader;
import com.iveely.robot.environment.Variable;

/**
 * @author {Iveely Liu}
 *
 */
public class Awareness {

	/**
	 * Start the brain consciousness, it must be kept in order to start.
	 */
	public static void wake() {
		// 1. wake up brain.
		Brain.getInstance();

		// 2. self-awakening.
		Self.getInstance();

		// 3. study or acquire some knowledge.
		SetLoader.getInstance().load();
		CategoryLoader.getInstance().load();

		// 4. activate nerve center to provide services.
		if (!Variable.isLocalMode()) {
			Nerve nerve = new Nerve();
			nerve.active();
		}
	}
}
