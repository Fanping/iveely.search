using System;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Services;

namespace NDatabase.Meta
{
    internal interface IMetaModel : IMetaModelService
    {
        void AddClass(ClassInfo classInfo);
        
        bool ExistClass(Type type);

        int GetNumberOfClasses();

        /// <summary>
        ///   Gets the class info from the OID.
        /// </summary>
        /// <remarks>
        ///   Gets the class info from the OID.
        /// </remarks>
        /// <param name="id"> </param>
        /// <returns> the class info with the OID </returns>
        ClassInfo GetClassInfoFromId(OID id);

        ClassInfo GetClassInfo(Type type, bool throwExceptionIfDoesNotExist);
        ClassInfo GetClassInfo(string fullClassName, bool throwExceptionIfDoesNotExist);

        /// <returns> The Last class info </returns>
        ClassInfo GetLastClassInfo();

        /// <param name="index"> The index of the class info to get </param>
        /// <returns> The class info at the specified index </returns>
        ClassInfo GetClassInfo(int index);

        void Clear();
        bool HasChanged();
        IEnumerable<ClassInfo> GetChangedClassInfo();
        void ResetChangedClasses();

        /// <summary>
        ///   Saves the fact that something has changed in the class (number of objects or last object oid)
        /// </summary>
        void AddChangedClass(ClassInfo classInfo);

        /// <summary>
        ///   Gets all the persistent classes that are subclasses or equal to the parameter class
        /// </summary>
        /// <returns> The list of class info of persistent classes that are subclasses or equal to the class </returns>
        IList<ClassInfo> GetPersistentSubclassesOf(Type type);
    }
}