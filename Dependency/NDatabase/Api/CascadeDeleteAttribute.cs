using System;

namespace NDatabase.Api
{
    /// <summary>
    /// Use when you want to do a cascade delete on field object. 
    /// </summary>
    /// <remarks>
    /// In such case, mark the attribute with <code>[CascadeDelete]</code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CascadeDeleteAttribute : Attribute
    {
    }
}