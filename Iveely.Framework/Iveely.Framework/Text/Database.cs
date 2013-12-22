/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using NDatabase;
using NDatabase.Api;

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
