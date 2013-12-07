using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NDatabase.Exceptions
{
    /// <summary>
    ///   ODB BTree Errors All @ in error description will be replaced by parameters
    /// </summary>
    internal sealed class BTreeError : IError
    {
        internal static readonly BTreeError LazyLoadingNode = new BTreeError(501,
                                                                           "Error while loading node lazily with oid @1");

        internal static readonly BTreeError InvalidIdForBtree = new BTreeError(504, "Invalid id for Btree : id=@1");

        internal static readonly BTreeError InternalError = new BTreeError(506, "Internal error: @1");

        private readonly int _code;

        private readonly string _description;

        private IList<string> _parameters;

        private BTreeError(int code, string description)
        {
            _code = code;
            _description = description;
        }

        #region IError Members

        public IError AddParameter<T>(T o) where T : class
        {
            var value = o != null
                            ? o.ToString()
                            : "null";

            return AddParameter(value);
        }

        public IError AddParameter(string s)
        {
            if (_parameters == null)
                _parameters = new List<string>();

            _parameters.Add(s);
            return this;
        }

        public IError AddParameter(int i)
        {
            return AddParameter(i.ToString(CultureInfo.InvariantCulture));
        }

        public IError AddParameter(byte i)
        {
            return AddParameter(i.ToString(CultureInfo.InvariantCulture));
        }

        public IError AddParameter(long l)
        {
            return AddParameter(l.ToString(CultureInfo.InvariantCulture));
        }

        #endregion

        /// <summary>
        ///   replace the @1,@2,...
        /// </summary>
        /// <remarks>
        ///   replace the @1,@2,... by their real values.
        /// </remarks>
        public override string ToString()
        {
            var buffer = new StringBuilder();
            buffer.Append(_code).Append(":").Append(_description);

            var token = buffer.ToString();

            if (_parameters != null)
            {
                for (var i = 0; i < _parameters.Count; i++)
                {
                    var parameterName = string.Concat("@", (i + 1).ToString());
                    var parameterValue = _parameters[i];
                    var parameterIndex = token.IndexOf(parameterName, StringComparison.Ordinal);

                    if (parameterIndex != -1)
                        token = ExceptionsHelper.ReplaceToken(token, parameterName, parameterValue, 1);
                }
            }

            return token;
        }
    }
}
