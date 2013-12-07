using System.Collections;
using System.Collections.Generic;
using System.Text;
using NDatabase.Api;

namespace NDatabase.Meta
{
    /// <summary>
    ///   A meta representation of an Array
    /// </summary>
    internal sealed class ArrayObjectInfo : NativeObjectInfo
    {
        private int _componentTypeId;
        private string _realArrayComponentClassName;

        public ArrayObjectInfo(IEnumerable array) : base(array, OdbType.ArrayId)
        {
            _realArrayComponentClassName = OdbType.DefaultArrayComponentClassName;
        }

        public ArrayObjectInfo(IEnumerable array, OdbType type, int componentId) : base(array, type)
        {
            _realArrayComponentClassName = OdbType.DefaultArrayComponentClassName;
            _componentTypeId = componentId;
        }

        public object[] GetArray()
        {
            return (object[]) TheObject;
        }

        public override string ToString()
        {
            if (TheObject != null)
            {
                var buffer = new StringBuilder();
                var array = GetArray();
                var length = array.Length;

                buffer.Append("[").Append(length).Append("]=(");

                for (var i = 0; i < length; i++)
                {
                    if (i != 0)
                        buffer.Append(",");

                    buffer.Append(array[i]);
                }

                buffer.Append(")");

                return buffer.ToString();
            }

            return "null array";
        }

        public override bool IsArrayObject()
        {
            return true;
        }

        public string GetRealArrayComponentClassName()
        {
            return _realArrayComponentClassName;
        }

        public void SetRealArrayComponentClassName(string realArrayComponentClassName)
        {
            _realArrayComponentClassName = realArrayComponentClassName;
        }

        public int GetArrayLength()
        {
            return GetArray().Length;
        }

        public void SetComponentTypeId(int componentTypeId)
        {
            _componentTypeId = componentTypeId;
        }

        public override AbstractObjectInfo CreateCopy(IDictionary<OID, AbstractObjectInfo> cache, bool onlyData)
        {
            var array = GetArray();
            var length = array.Length;

            var atomicNativeObjectInfos = new AtomicNativeObjectInfo[length];
            for (var i = 0; i < length; i++)
            {
                var aoi = (AbstractObjectInfo) array[i];
                atomicNativeObjectInfos[i] = aoi.CreateCopy(cache, onlyData) as AtomicNativeObjectInfo;
            }

            var arrayOfAoi = new ArrayObjectInfo(atomicNativeObjectInfos);
            arrayOfAoi.SetRealArrayComponentClassName(_realArrayComponentClassName);
            arrayOfAoi.SetComponentTypeId(_componentTypeId);

            return arrayOfAoi;
        }
    }
}
