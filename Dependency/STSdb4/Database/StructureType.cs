using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.Database
{
    public static class StructureType
    {
        //do not change
        public const int RESERVED = 0;

        public const int XTABLE = 1;
        public const int XFILE = 2;

        public static bool IsValid(int type)
        {
            if (type == XTABLE || type == XFILE)
                return true;

            return false;
        }
    }
}
