using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Iveely.Data
{
    public static class DataExtensions
    {
        public static Expression Value(this Expression data)
        {
            return Expression.Field(data, "Value");
        }
    }
}
