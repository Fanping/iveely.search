using System;

namespace NDatabase.TypeResolution
{
    /// <summary>
    /// Helper methods with regard to type resolution.
    /// </summary>
    internal static class TypeResolutionUtils
    {
        private static readonly ITypeResolver InternalTypeResolver
            = new CachedTypeResolver(new GenericTypeResolver());

        /// <summary>
        /// Resolves the supplied type name into a <see cref="System.Type"/>
        /// instance.
        /// </summary>
        /// <param name="typeName">
        /// The (possibly partially assembly qualified) name of a
        /// <see cref="System.Type"/>.
        /// </param>
        /// <returns>
        /// A resolved <see cref="System.Type"/> instance.
        /// </returns>
        /// <exception cref="System.TypeLoadException">
        /// If the type cannot be resolved.
        /// </exception>
        public static Type ResolveType(string typeName)
        {
            return TypeRegistry.ResolveType(typeName) ?? InternalTypeResolver.Resolve(typeName);
        }
    }
}