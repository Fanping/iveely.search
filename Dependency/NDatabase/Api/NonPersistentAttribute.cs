using System;

namespace NDatabase.Api
{
    /// <summary>
    /// Use when you don't want to serialize the field. 
    /// </summary>
    /// <remarks>
    /// In such case, mark the attribute with <code>[NonPersistent]</code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NonPersistentAttribute : Attribute
    {
    }
}