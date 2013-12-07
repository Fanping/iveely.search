namespace NDatabase.TypeResolution
{
    /// <summary>
    /// Holds data about a <see cref="System.Type"/> and it's
    /// attendant <see cref="System.Reflection.Assembly"/>.
    /// </summary>
    internal sealed class TypeAssemblyHolder
    {
        /// <summary>
        /// The string that separates a <see cref="System.Type"/> name
        /// from the name of it's attendant <see cref="System.Reflection.Assembly"/>
        /// in an assembly qualified type name.
        /// </summary>
        private const string TypeAssemblySeparator = ",";

        private string _unresolvedAssemblyName;
        private string _unresolvedTypeName;

        /// <summary>
        /// Creates a new instance of the TypeAssemblyHolder class.
        /// </summary>
        /// <param name="unresolvedTypeName">
        /// The unresolved name of a <see cref="System.Type"/>.
        /// </param>
        public TypeAssemblyHolder(string unresolvedTypeName)
        {
            SplitTypeAndAssemblyNames(unresolvedTypeName);
        }

        /// <summary>
        /// The (unresolved) type name portion of the original type name.
        /// </summary>
        public string GetTypeName()
        {
            return _unresolvedTypeName;
        }

        /// <summary>
        /// The (unresolved, possibly partial) name of the attendant assembly.
        /// </summary>
        public string GetAssemblyName()
        {
            return _unresolvedAssemblyName;
        }

        /// <summary>
        /// Is the type name being resolved assembly qualified?
        /// </summary>
        public bool IsAssemblyQualified
        {
            get { return !string.IsNullOrWhiteSpace(GetAssemblyName()); }
        }

        private void SplitTypeAndAssemblyNames(string originalTypeName)
        {
            // generic types may look like:
            // Spring.Objects.TestGenericObject`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]][] , Spring.Core.Tests, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
            //
            // start searching for assembly separator after the last bracket if any
            var typeAssemblyIndex = originalTypeName.LastIndexOf(']');
            typeAssemblyIndex = originalTypeName.IndexOf(TypeAssemblySeparator, typeAssemblyIndex+1, System.StringComparison.Ordinal);
            if (typeAssemblyIndex < 0)
            {
                _unresolvedTypeName = originalTypeName;
            }
            else
            {
                _unresolvedTypeName = originalTypeName.Substring(
                    0, typeAssemblyIndex).Trim();
                _unresolvedAssemblyName = originalTypeName.Substring(
                    typeAssemblyIndex + 1).Trim();
            }
        }
    }
}
