using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iveely.General
{
    public static class Environment
    {
        public static readonly bool RunningOnMono = Type.GetType("Mono.Runtime") != null;
    }
}
