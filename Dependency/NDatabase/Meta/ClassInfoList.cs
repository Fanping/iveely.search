using System;
using System.Collections.Generic;
using System.Text;

namespace NDatabase.Meta
{
    /// <summary>
    ///   A simple list to contain some class infos.
    /// </summary>
    /// <remarks>
    ///   A simple list to contain some class infos. 
    ///  <pre>It used by ClassIntropector.introspect to return all the class info detected by introspecting a class.
    ///       For example, if we have a class Class1 that has a field of type Class2. And Class2 has a field of type Class3.
    ///       Introspecting Class1 return a ClassInfoList with the classes Class1, Class2, Class3. Class1 being the main class info</pre>
    /// </remarks>
    internal sealed class ClassInfoList
    {
        private readonly IDictionary<Type, ClassInfo> _classInfosByType;

        private readonly ClassInfo _mainClassInfo;

        public ClassInfoList(ClassInfo mainClassInfo)
        {
            _classInfosByType = new Dictionary<Type, ClassInfo> {{mainClassInfo.UnderlyingType, mainClassInfo}};
            _mainClassInfo = mainClassInfo;
        }

        public ClassInfo GetMainClassInfo()
        {
            return _mainClassInfo;
        }

        public void AddClassInfo(ClassInfo classInfo)
        {
            _classInfosByType.Add(classInfo.UnderlyingType, classInfo);
        }

        public ICollection<ClassInfo> GetClassInfos()
        {
            return _classInfosByType.Values;
        }

        /// <returns> null if it does not exist </returns>
        public ClassInfo GetClassInfoBy(Type type)
        {
            ClassInfo classInfo;
            _classInfosByType.TryGetValue(type, out classInfo);
            return classInfo;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append(_classInfosByType.Count).Append(" - ").Append(_classInfosByType.Keys);
            return buffer.ToString();
        }
    }
}
