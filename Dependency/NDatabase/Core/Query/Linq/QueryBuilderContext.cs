using System;
using System.Collections.Generic;
using NDatabase.Api.Query;

namespace NDatabase.Core.Query.Linq
{
    internal sealed class QueryBuilderContext
    {
        private readonly Stack<IConstraint> _constraints = new Stack<IConstraint>();
        private readonly Stack<IQuery> _descendStack = new Stack<IQuery>();
        private readonly IQuery _root;

        private Type _descendigFieldEnum;

        public QueryBuilderContext(IQuery root)
        {
            _root = root;
            CurrentQuery = _root;
        }

        public IQuery CurrentQuery { get; private set; }

        internal void PushDescendigFieldEnumType(Type descendigFieldEnum)
        {
            _descendigFieldEnum = descendigFieldEnum;
        }

        private Type PopDescendigFieldEnumType()
        {
            var returnType = _descendigFieldEnum;
            _descendigFieldEnum = null;

            return returnType;
        }

        public void PushConstraint(IConstraint constraint)
        {
            _constraints.Push(constraint);
        }

        public IConstraint PopConstraint()
        {
            return _constraints.Pop();
        }

        public void ApplyConstraint(Func<IConstraint, IConstraint> constraint)
        {
            PushConstraint(constraint(PopConstraint()));
        }

        internal object ResolveValue(object value)
        {
            var type = PopDescendigFieldEnumType();
            
            return type != null
                ? Enum.ToObject(type, value) 
                : value;
        }

        public void Descend(string name)
        {
            CurrentQuery = CurrentQuery.Descend(name);
        }

        public void SaveQuery()
        {
            _descendStack.Push(CurrentQuery);
        }

        public void RestoreQuery()
        {
            CurrentQuery = _descendStack.Pop();
        }
    }
}
