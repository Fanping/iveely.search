package com.iveely.computing.command;

/**
 * The interface for command process.
 *
 * @author liufanping@iveely.com
 * @date 2014-10-18 14:12:25
 */
public interface IMandate {

    /**
     * process command.
     *
     * @param cmd cmd
     * @return process result.
     */
    public String processCmd(String cmd);

}
