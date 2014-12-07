package iveely.search.plugin.math;

import com.iveely.framework.net.ICallback;
import com.iveely.framework.net.InternetPacket;
import com.iveely.framework.text.StringUtils;

/**
 * Call back of plugin service.
 *
 * @author liufanping@iveely.com
 * @date 2014-11-16 11:29:17
 */
public class CallBack implements ICallback {

    /**
     * Calculator
     */
    private final Calculator calculator;

    public CallBack() {
        calculator = new Calculator();
    }

    @Override
    public InternetPacket invoke(InternetPacket packet) {
        String expression = StringUtils.getString(packet.getData()).replace(" ", "");
        String result = String.valueOf(calculator.calculate(expression));
        packet.setData(StringUtils.getBytes(result));
        return packet;
    }
}
