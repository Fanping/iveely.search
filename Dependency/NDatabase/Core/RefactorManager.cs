using System;
using System.Reflection;
using NDatabase.Api;
using NDatabase.Meta;

namespace NDatabase.Core
{
    internal sealed class RefactorManager : IRefactorManager
    {
        private readonly IMetaModel _metaModel;
        private readonly IObjectWriter _objectWriter;

        internal RefactorManager(IMetaModel metaModel, IObjectWriter objectWriter)
        {
            _metaModel = metaModel;
            _objectWriter = objectWriter;
        }

        #region IRefactorManager Members

        public void AddField(Type type, Type fieldType, string fieldName)
        {
            var classInfo = _metaModel.GetClassInfo(type, true);

            // The real attribute id (-1) will be set in the ci.addAttribute
            var fullClassName = OdbClassNameResolver.GetFullName(fieldType);
            var attributeInfo = new ClassAttributeInfo(-1, fieldName, fullClassName, classInfo);
            classInfo.AddAttribute(attributeInfo);

            _objectWriter.UpdateClassInfo(classInfo, true);
        }

        public void RemoveField(Type type, string attributeName)
        {
            var classInfo = _metaModel.GetClassInfo(type, true);
            var attributeInfo = classInfo.GetAttributeInfoFromName(attributeName);

            classInfo.RemoveAttribute(attributeInfo);

            _objectWriter.UpdateClassInfo(classInfo, true);
        }

        public void RenameClass(string fullClassName, Type newType)
        {
            var classInfo = _metaModel.GetClassInfo(fullClassName, true);

            var fullClassNameField = classInfo.GetType().GetField("_fullClassName",
                                                            BindingFlags.Instance | BindingFlags.NonPublic);
            var newFullClassName = OdbClassNameResolver.GetFullName(newType);
            fullClassNameField.SetValue(classInfo, newFullClassName);

            var underlyingTypeField = classInfo.GetType()
                                               .GetField("_underlyingType",
                                                         BindingFlags.Instance | BindingFlags.NonPublic);
            underlyingTypeField.SetValue(classInfo, newType);

            _objectWriter.UpdateClassInfo(classInfo, true);
        }

        public void RenameField(Type type, string attributeName, string newAttributeName)
        {
            var classInfo = _metaModel.GetClassInfo(type, true);
            var attributeInfo = classInfo.GetAttributeInfoFromName(attributeName);

            classInfo.RemoveAttribute(attributeInfo);
            attributeInfo.SetName(newAttributeName);
            classInfo.AddAttribute(attributeInfo);

            _objectWriter.UpdateClassInfo(classInfo, true);
        }

        #endregion
    }
}