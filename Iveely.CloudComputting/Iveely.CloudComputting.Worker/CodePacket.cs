/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using Iveely.Framework.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class _CodePacket : Packet
{
    public string className
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

    public _CodePacket(byte[] codeBytes, string className, string appName, string timeStamp)
    {
        this.Data = codeBytes;
        this.className = className;
        this.AppName = appName;
        this.TimeStamp = timeStamp;
    }
}
