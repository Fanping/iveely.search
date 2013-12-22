// ------------------------------------------------------------------------------------------------
//  <copyright file="InitialStateProbability.cs" company="Iveely">
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
    /// <summary>
    /// 初试状态概率
    /// </summary>
    [Serializable]
    public class InitialStateProbability
    {
        /// <summary>
        /// 初始状态概率表
        /// </summary>
        public IntTable<string, double> Table;

        public InitialStateProbability()
        {
            Table = new IntTable<string, double>();
        }

        public void Add(string state, double probability)
        {
            Table.Add(state, probability);
        }

        public double this[string index]
        {
            get { return double.Parse(Table[index].ToString()); }
        }
    }
}