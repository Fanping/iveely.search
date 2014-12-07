package com.iveely.computing.command;

/**
 * Unknown command.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:15:38
 */
public class CmdUnknown implements IMandate {

    @Override
    public String processCmd(String cmd) {
        return "Unknow mandate:" + cmd;
    }
}
