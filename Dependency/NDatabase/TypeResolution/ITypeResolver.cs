using System;

namespace NDatabase.TypeResolution
{
	/// <summary>
	/// Resolves a <see cref="System.Type"/> by name.
	/// </summary>
	/// <remarks>
	/// <p>
	/// The rationale behind the creation of this interface is to centralize
	/// the resolution of type names to <see cref="System.Type"/> instances
	/// beyond that offered by the plain vanilla
	/// <see cref="System.Type.GetType(string)"/> method call.
	/// </p>
	/// </remarks>
	internal interface ITypeResolver
	{
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
		Type Resolve(string typeName);
	}
}
