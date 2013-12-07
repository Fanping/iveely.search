using System;
using System.Collections.Generic;
using NDatabase.Api;
using NDatabase.Exceptions;
using NDatabase.Meta;

namespace NDatabase.Indexing
{
    internal static class IndexTool
    {
        internal static IOdbComparable BuildIndexKey(string indexName, NonNativeObjectInfo oi, int[] fieldIds)
        {
            var keys = new IOdbComparable[fieldIds.Length];

            for (var i = 0; i < fieldIds.Length; i++)
            {
                try
                {
                    var aoi = oi.GetAttributeValueFromId(fieldIds[i]);
                    var item = (IComparable) aoi.GetObject();
                    
                    // If the index is on NonNativeObjectInfo, then the key is the oid of the object
                    if (aoi.IsNonNativeObject())
                    {
                        var nnoi = (NonNativeObjectInfo) aoi;
                        item = nnoi.GetOid();
                    }

                    keys[i] = new SimpleCompareKey(item);
                }
                catch (Exception)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.IndexKeysMustImplementComparable.AddParameter(indexName).AddParameter(fieldIds[i]).AddParameter(
                            oi.GetAttributeValueFromId(fieldIds[i]).GetType().FullName));
                }
            }

            return keys.Length == 1 ? keys[0] : new ComposedCompareKey(keys);
        }

        internal static IOdbComparable BuildIndexKey(string indexName, AttributeValuesMap values, IList<string> fields)
        {
            if (fields.Count == 1)
                return new SimpleCompareKey(values.GetComparable(fields[0]));

            var keys = new IOdbComparable[fields.Count];
            for (var i = 0; i < fields.Count; i++)
            {
                try
                {
                    var @object = (IComparable) values[fields[i]];
                    keys[i] = new SimpleCompareKey(@object);
                }
                catch (Exception)
                {
                    throw new OdbRuntimeException(
                        NDatabaseError.IndexKeysMustImplementComparable.AddParameter(indexName).AddParameter(fields[i]).
                            AddParameter(values[fields[i]].GetType().FullName));
                }
            }

            return new ComposedCompareKey(keys);
        }
    }
}
