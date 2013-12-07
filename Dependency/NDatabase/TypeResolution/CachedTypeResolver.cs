using System;
using System.Collections.Generic;
using NDatabase.Tool;

namespace NDatabase.TypeResolution
{
	/// <summary>
	/// Resolves (instantiates) a <see cref="System.Type"/> by it's (possibly
	/// assembly qualified) name, and caches the <see cref="System.Type"/>
	/// instance against the type name.
	/// </summary>
	internal sealed class CachedTypeResolver : ITypeResolver
	{
		/// <summary>
		/// The cache, mapping type names (<see cref="System.String"/> instances) against
		/// <see cref="System.Type"/> instances.
		/// </summary>
        private readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

		private readonly ITypeResolver _typeResolver;

		/// <summary>
		/// Creates a new instance of the <see cref="CachedTypeResolver"/> class.
		/// </summary>
		/// <param name="typeResolver">
		/// The <see cref="ITypeResolver"/> that this instance will delegate
		/// actual <see cref="System.Type"/> resolution to if a <see cref="System.Type"/>
		/// cannot be found in this instance's <see cref="System.Type"/> cache.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// If the supplied <paramref name="typeResolver"/> is <see langword="null"/>.
		/// </exception>
		public CachedTypeResolver(ITypeResolver typeResolver)
		{
		    if (typeResolver == null)
		        throw new ArgumentNullException("typeResolver");

		    _typeResolver = typeResolver;
		}

		/// <summary>
		/// Resolves the supplied <paramref name="typeName"/> to a
		/// <see cref="System.Type"/>
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
		/// If the supplied <paramref name="typeName"/> could not be resolved
		/// to a <see cref="System.Type"/>.
		/// </exception>
		public Type Resolve(string typeName)
		{
		    if (string.IsNullOrEmpty(typeName))
		        throw BuildTypeLoadException(typeName);

		    try
			{
                return _typeCache.GetOrAdd(typeName, _typeResolver.Resolve);
			}
			catch (Exception ex)
			{
			    if (ex is TypeLoadException)
			        throw;

			    throw BuildTypeLoadException(typeName, ex);
			}
		}

		private static TypeLoadException BuildTypeLoadException(string typeName)
		{
			return new TypeLoadException("Could not load type from string value '" + typeName + "'.");
		}

        private static TypeLoadException BuildTypeLoadException(string typeName, Exception ex)
        {
            return new TypeLoadException("Could not load type from string value '" + typeName + "'.", ex);
        }
	}
}
