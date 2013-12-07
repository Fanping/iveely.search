/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NDatabase;
using NDatabase.Api;
using NDatabase.Container;
using NDatabase.Core;
using NDatabase.Core.Engine;
using NDatabase.Core.Query;
using NDatabase.Core.Session;
using NDatabase.Meta;
using NDatabase.Services;
using NDatabase.Transaction;

namespace Iveely.Framework.Text
{
    public class Database
    {
        public static IOdb Open(string fileName)
        {
            return OdbFactory.Open(fileName);
        }

        public static IOdb OpenLast()
        {
            return OdbFactory.OpenLast();
        }

        public static IOdb OpenInMemory()
        {
            return OdbFactory.OpenInMemory();
        }

        public static void Delete(string fileName)
        {
            OdbFactory.Delete(fileName);
        }
    }
}
