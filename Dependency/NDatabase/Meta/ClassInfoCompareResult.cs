using System.Text;
using NDatabase.Tool;
using NDatabase.Tool.Wrappers;

namespace NDatabase.Meta
{
    /// <summary>
    ///   To keep track of differences between two ClassInfo.
    /// </summary>
    /// <remarks>
    ///   To keep track of differences between two ClassInfo. Ussed by the MetaModel compatibility checker
    /// </remarks>
    internal sealed class ClassInfoCompareResult
    {
        private readonly string _fullClassName;

        private readonly IOdbList<string> _compatibleChanges;
        private readonly IOdbList<string> _incompatibleChanges;

        public ClassInfoCompareResult(string fullClassName)
        {
            _fullClassName = fullClassName;
            _incompatibleChanges = new OdbList<string>(5);
            _compatibleChanges = new OdbList<string>(5);
        }

        public bool IsCompatible()
        {
            return _incompatibleChanges.IsEmpty();
        }

        public override string ToString()
        {
            var buffer = new StringBuilder();

            buffer.Append(_fullClassName).Append(" is Compatible = ").Append(IsCompatible()).Append("\n");
            buffer.Append("Incompatible changes = ").Append(_incompatibleChanges);
            buffer.Append("\nCompatible changes = ").Append(_compatibleChanges);

            return buffer.ToString();
        }

        public void AddCompatibleChange(string o)
        {
            _compatibleChanges.Add(o);
        }

        public void AddIncompatibleChange(string o)
        {
            _incompatibleChanges.Add(o);
        }

        public bool HasCompatibleChanges()
        {
            return !_compatibleChanges.IsEmpty();
        }

        public string GetFullClassName()
        {
            return _fullClassName;
        }
    }
}
