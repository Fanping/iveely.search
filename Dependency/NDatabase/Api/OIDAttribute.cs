using System;

namespace NDatabase.Api
{

    /// <summary>
    /// Use when you want to enrich your class with OID.
    /// You can apply it on fields of type: long or OID.
    /// </summary>
    /// <remarks>
    /// In such case, mark the attribute with <code>[OID]</code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class OIDAttribute : Attribute
    {
    }
}