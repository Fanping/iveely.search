using System;
using System.Reflection;

namespace NDatabase.TypeResolution
{
    /// <summary>
    /// Resolves a <see cref="System.Type"/> by name.
    /// </summary>
    internal class TypeResolver : ITypeResolver
    {
        /// <summary>
        /// Resolves the supplied <paramref name="typeName"/> to a
        /// <see cref="System.Type"/> instance.
        /// </summary>
        /// <param name="typeName">
        /// The unresolved (possibly partially assembly qualified) name 
        /// of a <see cref="System.Type"/>.
        /// </param>
        /// <returns>
        /// A resolved <see cref="System.Type"/> instance.
        /// </returns>
        /// <exception cref="System.TypeLoadException">
        /// If the supplied <paramref name="typeName"/> could not be resolved
        /// to a <see cref="System.Type"/>.
        /// </exception>
        public virtual Type Resolve(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw BuildTypeLoadException(typeName);

            var typeInfo = new TypeAssemblyHolder(typeName);
            Type type;
            try
            {
                type = (typeInfo.IsAssemblyQualified) ?
                     LoadTypeDirectlyFromAssembly(typeInfo) :
                     LoadTypeByIteratingOverAllLoadedAssemblies(typeInfo);
            }
            catch
            {
                try
                {
                    type = LoadTypeByIteratingOverAllLoadedAssemblies(typeInfo);
                }
                catch (Exception ex)
                {
                    if (ex is TypeLoadException)
                        throw;

                    throw BuildTypeLoadException(typeName, ex);
                }
            }
            
            return type;
        }

        private static Type LoadTypeDirectlyFromAssembly(TypeAssemblyHolder typeInfo)
        {
            Type type = null;

            var assembly = Assembly.Load(typeInfo.GetAssemblyName());

            if (assembly != null)
                type = assembly.GetType(typeInfo.GetTypeName(), true, true);

            return type;
        }

        private static Type LoadTypeByIteratingOverAllLoadedAssemblies(TypeAssemblyHolder typeInfo)
        {
            Type type = null;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(typeInfo.GetTypeName(), false, false);
                if (type != null)
                    break;
            }
            return type;
        }

        /// <summary>
        /// Creates a new <see cref="TypeLoadException"/> instance 
        /// from the given <paramref name="typeName"/>
        /// </summary>
        protected static TypeLoadException BuildTypeLoadException(string typeName)
        {
            return new TypeLoadException("Could not load type from string value '" + typeName + "'.");
        }

        /// <summary>
        /// Creates a new <see cref="TypeLoadException"/> instance
        /// from the given <paramref name="typeName"/> with the given inner <see cref="Exception"/> 
        /// </summary>
        protected static TypeLoadException BuildTypeLoadException(string typeName, Exception ex)
        {
            return new TypeLoadException("Could not load type from string value '" + typeName + "'.", ex);
        }
    }
}
