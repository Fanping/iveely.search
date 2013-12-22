// ------------------------------------------------------------------------------------------------
//  <copyright file="State.cs" company="Iveely">
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

#endregion

namespace Iveely.Framework.Text.Segment
{
    /// <summary>
    /// 状态序列
    /// </summary>
    [Serializable]
    public class State : Sequence
    {
        public int Length
        {
            get { return Array.Count; }
        }

        public string this[int index]
        {
            get { return Array[index]; }
        }
    }
}