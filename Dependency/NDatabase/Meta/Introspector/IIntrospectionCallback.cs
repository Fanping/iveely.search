namespace NDatabase.Meta.Introspector
{
    internal interface IIntrospectionCallback
    {
        /// <summary>
        ///   Called when the introspector find a non native object.
        /// </summary>
        void ObjectFound(object @object);
    }
}