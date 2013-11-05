// ------------------------------------------------------------------------------------------------
//  <copyright file="Sequence.cs" company="Iveely">
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
using System.Collections.Generic;

#endregion

namespace Iveely.Framework.Text.Segment
{
    [Serializable]
    public class Sequence
    {
        /// <summary>
        ///   序列
        /// </summary>
        public List<string> array { get; private set; }

        public Sequence()
        {
            array = new List<string>();
        }

        public void Add(string value)
        {
            if(!array.Contains(value))
            {
                this.array.Add(value);
            }
        }
    }
}