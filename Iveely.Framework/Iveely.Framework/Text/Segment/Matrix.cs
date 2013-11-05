// ------------------------------------------------------------------------------------------------
//  <copyright file="Matrix.cs" company="Iveely">
//    Copyright (c) Iveely Liu.  All rights reserved.
//  </copyright>
//  
//  <Create Time>
//    03/02/2013 21:59 
//  </Create Time>
//  
//  <contact owner>
//    liufanping@iveely.com 
//  </contact owner>
//  -----------------------------------------------------------------------------------------------

#region

using System;
using Iveely.Framework.DataStructure;

#endregion

namespace Iveely.Framework.Text.Segment
{
    [Serializable]
    public class Matrix
    {
        public DimensionTable<string, string, double> Table { get; private set; }

        public Matrix()
        {
            Table = new DimensionTable<string, string, double>();
        }
    }
}