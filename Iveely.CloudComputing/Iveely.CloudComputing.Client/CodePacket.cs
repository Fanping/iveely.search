/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iveely.Framework.Network;

[Serializable]
public class ExcutePacket : Packet
{
    public enum Type
    {
        Code,
        Kill,
        FileFragment,
        Download,
        Delete,
        Rename,
        List
    }

    public string ClassName
    {
        get;
        private set;
    }

    public string AppName
    {
        get;
        private set;
    }

    public string TimeStamp
    {
        get;
        private set;
    }

    /// <summary>
    /// 信息返回IP
    /// </summary>
    public string ReturnIp
    {
        get;
        private set;
    }

    /// <summary>
    /// 信息返回接收端口
    /// </summary>
    public int Port
    {
        get;
        private set;
    }

    /// <summary>
    /// 执行类型
    /// </summary>
    public Type ExcuteType
    {
        get;
        private set;
    }

    public ExcutePacket(byte[] codeBytes, string className, string appName, string timeStamp, Type excuteType)
    {
        this.Data = codeBytes;
        this.ClassName = className;
        this.AppName = appName;
        this.TimeStamp = timeStamp;
        this.ExcuteType = excuteType;
    }

    public ExcutePacket()
    {}

    public void SetReturnAddress(string ip, int port)
    {
        this.ReturnIp = ip;
        this.Port = port;
    }
}

