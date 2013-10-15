/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.CloudComputting.StateCommon
{
    [Serializable]
    public enum ExcuteType
    {
        FileCreate,
        FileRead,
        FileWrite,
        Average,
        Sum,
        Distinct,
        EndToEnd
    }
}
