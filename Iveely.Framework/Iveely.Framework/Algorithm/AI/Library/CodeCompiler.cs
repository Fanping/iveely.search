/////////////////////////////////////////////////
///文件名:CodeCompiler
///描  述:
///创建者:刘凡平(Iveely Liu)
///邮  箱:liufanping@iveely.com
///组  织:Iveely
///年  份:2012/3/28 16:05:24
///////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Reflection;

namespace Iveely.Framework.Algorithm.AI.Library
{
    /// <summary>
    /// 代码编译
    /// </summary>
    public class CodeCompiler
    {
        /// <summary>
        /// 动态执行
        /// </summary>
        /// <param name="funName">函数名称</param>
        /// <param name="objs">参数集 </param>
        /// <returns>返回执行结果</returns>
        public static string Execute(string funName, object[] objs)
        {
            Type magicType = Type.GetType("Iveely.Framework.Algorithm.AI.Library.Sys");
            Debug.Assert(magicType != null, "magicType != null");
            ConstructorInfo magicConstructor = magicType.GetConstructor(Type.EmptyTypes);
            Debug.Assert(magicConstructor != null, "magicConstructor != null");
            object magicClassObject = magicConstructor.Invoke(new object[] { });
            MethodInfo magicMethod = magicType.GetMethod(funName);
            object magicValue = magicMethod.Invoke(magicClassObject, objs);
            //object magicValue = magicMethod.Invoke(magicClassObject,null);
            return magicValue.ToString();
        }
    }
}
