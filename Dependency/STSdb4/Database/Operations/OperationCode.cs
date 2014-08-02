using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveely.STSdb4.WaterfallTree;
using Iveely.STSdb4.Data;

namespace Iveely.STSdb4.Database.Operations
{
    public static class OperationCode
    {
        public const int UNDEFINED = 0;

        //XIndex
        public const int REPLACE = 1;
        public const int INSERT_OR_IGNORE = 2;
        public const int DELETE = 3;
        public const int DELETE_RANGE = 4;
        public const int CLEAR = 5;

        public const int MAX = 256;
    }    
}
