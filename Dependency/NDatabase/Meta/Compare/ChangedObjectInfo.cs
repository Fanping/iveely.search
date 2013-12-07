using System.Text;

namespace NDatabase.Meta.Compare
{
    /// <summary>
    ///   Used to store informations about object changes
    /// </summary>
    internal sealed class ChangedObjectInfo
    {
        private readonly int _fieldIndex;

        private readonly string _message;
        private readonly ClassInfo _newCi;
        private readonly AbstractObjectInfo _newValue;

        private readonly int _objectRecursionLevel;
        private readonly ClassInfo _oldCi;
        private readonly AbstractObjectInfo _oldValue;

        public ChangedObjectInfo(ClassInfo oldCi, ClassInfo newCi, int fieldIndex, AbstractObjectInfo oldValue,
                                 AbstractObjectInfo newValue, int objectRecursionLevel)
            : this(oldCi, newCi, fieldIndex, oldValue, newValue, null, objectRecursionLevel)
        {
        }

        public ChangedObjectInfo(ClassInfo oldCi, ClassInfo newCi, int fieldIndex, AbstractObjectInfo oldValue,
                                 AbstractObjectInfo newValue, string message, int objectRecursionLevel)
        {
            _oldCi = oldCi;
            _newCi = newCi;
            _fieldIndex = fieldIndex;
            _oldValue = oldValue;
            _newValue = newValue;
            _message = message;
            _objectRecursionLevel = objectRecursionLevel;
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            if (_message != null)
                buffer.Append(_message).Append(" | ");

            if (_oldCi.ClassInfoId != _newCi.ClassInfoId)
                buffer.Append("old class=").Append(_oldCi.FullClassName).Append(" | new class=").Append(_newCi.FullClassName);
            else
                buffer.Append("class=").Append(_oldCi.FullClassName);

            buffer.Append(" | field=").Append(_oldCi.GetAttributeInfo(_fieldIndex).GetName());
            buffer.Append(" | old=").Append(_oldValue).Append(" | new=").Append(_newValue);
            buffer.Append(" | obj. hier. level=").Append(_objectRecursionLevel);

            return buffer.ToString();
        }
    }
}
