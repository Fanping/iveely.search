using System;
using System.Text;

namespace NDatabase.Meta
{
    /// <summary>
    ///   to keep informations about an attribute of a class : 
    ///    - Its type
    ///    - its name
    ///    - If it is an index
    /// </summary>
    internal sealed class ClassAttributeInfo
    {
        private OdbType _attributeType;

        private ClassInfo _classInfo;

        private string _className;

        private string _fullClassName;
        private int _id;
        private bool _isIndex;
        private string _name;
        private string _namespace;

        internal ClassAttributeInfo()
        {
        }

        internal ClassAttributeInfo(int attributeId, string name, string fullClassName, ClassInfo info)
            : this(attributeId, name, null, fullClassName, info)
        {
        }

        internal ClassAttributeInfo(int attributeId, string name, Type nativeClass, string fullClassName, ClassInfo info)
        {
            _id = attributeId;
            _name = name;
            SetFullClassName(fullClassName);
            
            if (nativeClass != null)
            {
                _attributeType = OdbType.GetFromClass(nativeClass);
            }
            else
            {
                if (fullClassName != null)
                    _attributeType = OdbType.GetFromName(fullClassName);
            }

            _classInfo = info;
            _isIndex = false;
        }

        internal ClassInfo GetClassInfo()
        {
            return _classInfo;
        }

        internal void SetClassInfo(ClassInfo classInfo)
        {
            _classInfo = classInfo;
        }

        internal bool IsIndex()
        {
            return _isIndex;
        }

        internal void SetIndex(bool isIndex)
        {
            _isIndex = isIndex;
        }

        internal string GetName()
        {
            return _name;
        }

        internal void SetName(string name)
        {
            _name = name;
        }

        internal bool IsNative()
        {
            return _attributeType.IsNative();
        }

        internal bool IsNonNative()
        {
            return !_attributeType.IsNative();
        }

        internal void SetFullClassName(string fullClassName)
        {
            _fullClassName = fullClassName;
            _className = OdbClassNameResolver.GetClassName(fullClassName);
            _namespace = OdbClassNameResolver.GetNamespace(fullClassName);
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append("id=").Append(_id).Append(" name=").Append(_name).Append(" | is Native=").Append(IsNative()).
                Append(" | type=").Append(GetFullClassname()).Append(" | isIndex=").Append(_isIndex);

            return buffer.ToString();
        }

        internal string GetFullClassname()
        {
            if (_fullClassName != null)
                return _fullClassName;

            if (string.IsNullOrEmpty(_namespace))
            {
                _fullClassName = _className;
                return _className;
            }

            _fullClassName = string.Format("{0}.{1}", _namespace, _className);
            return _fullClassName;
        }

        internal void SetAttributeType(OdbType attributeType)
        {
            _attributeType = attributeType;
        }

        internal OdbType GetAttributeType()
        {
            return _attributeType;
        }

        internal int GetId()
        {
            return _id;
        }

        internal void SetId(int id)
        {
            _id = id;
        }
    }
}
