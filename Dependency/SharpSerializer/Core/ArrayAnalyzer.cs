#region Copyright � 2010 Pawel Idzikowski [idzikowski@sharpserializer.com]

//  ***********************************************************************
//  Project: sharpSerializer
//  Web: http://www.sharpserializer.com
//  
//  This software is provided 'as-is', without any express or implied warranty.
//  In no event will the author(s) be held liable for any damages arising from
//  the use of this software.
//  
//  Permission is granted to anyone to use this software for any purpose,
//  including commercial applications, and to alter it and redistribute it
//  freely, subject to the following restrictions:
//  
//      1. The origin of this software must not be misrepresented; you must not
//        claim that you wrote the original software. If you use this software
//        in a product, an acknowledgment in the product documentation would be
//        appreciated but is not required.
//  
//      2. Altered source versions must be plainly marked as such, and must not
//        be misrepresented as being the original software.
//  
//      3. This notice may not be removed or altered from any source distribution.
//  
//  ***********************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Iveely.Dependency.Polenter.Serialization.Core
{
    /// <summary>
    ///   Gives information about actually analysed array (from the constructor)
    /// </summary>
    public class ArrayAnalyzer
    {
        private readonly object _array;
        private readonly ArrayInfo _arrayInfo;
        private IList<int[]> _indexes;

        ///<summary>
        ///</summary>
        ///<param name = "array"></param>
        public ArrayAnalyzer(object array)
        {
            _array = array;
            var type = array.GetType();
            _arrayInfo = getArrayInfo(type);
        }

        /// <summary>
        ///   Contains extended information about the current array
        /// </summary>
        public ArrayInfo ArrayInfo
        {
            get { return _arrayInfo; }
        }

        /// <summary>
        ///   How many dimensions. There can be at least 1
        /// </summary>
        /// <returns></returns>
        private int getRank(Type arrayType)
        {
            return arrayType.GetArrayRank();
        }

        /// <summary>
        ///   How many items in one dimension
        /// </summary>
        /// <param name = "dimension">0-based</param>
        /// <returns></returns>
        /// <param name="arrayType"></param>
        private int getLength(int dimension, Type arrayType)
        {
            MethodInfo methodInfo = arrayType.GetMethod("GetLength");
            var length = (int) methodInfo.Invoke(_array, new object[] {dimension});
            return length;
        }

        /// <summary>
        ///   Lower index of an array. Default is 0.
        /// </summary>
        /// <param name = "dimension">0-based</param>
        /// <returns></returns>
        /// <param name="arrayType"></param>
        private int getLowerBound(int dimension, Type arrayType)
        {
            return getBound("GetLowerBound", dimension, arrayType);
        }


//        private int getUpperBound(int dimension)
//        {
        // Not used, as UpperBound is equal LowerBound+Length
//            return getBound("GetUpperBound", dimension);
//        }

        private int getBound(string methodName, int dimension, Type arrayType)
        {
            MethodInfo methodInfo = arrayType.GetMethod(methodName);
            var bound = (int) methodInfo.Invoke(_array, new object[] {dimension});
            return bound;
        }

        private ArrayInfo getArrayInfo(Type arrayType)
        {
            // Caching is innacceptable, as an array of type string can have different bounds

            var info = new ArrayInfo();

            // Fill the dimension infos
            for (int dimension = 0; dimension < getRank(arrayType); dimension++)
            {
                var dimensionInfo = new DimensionInfo();
                dimensionInfo.Length = getLength(dimension, arrayType);
                dimensionInfo.LowerBound = getLowerBound(dimension, arrayType);
                info.DimensionInfos.Add(dimensionInfo);
            }


            return info;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public IEnumerable<int[]> GetIndexes()
        {
            if (_indexes == null)
            {
                _indexes = new List<int[]>();
                ForEach(addIndexes);
            }

            foreach (var item in _indexes)
            {
                yield return item;
            }
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public IEnumerable<object> GetValues()
        {
            foreach (var indexSet in GetIndexes())
            {
                object value = ((Array) _array).GetValue(indexSet);
                yield return value;
            }
        }

        private void addIndexes(int[] obj)
        {
            _indexes.Add(obj);
        }


        ///<summary>
        ///</summary>
        ///<param name = "action"></param>
        public void ForEach(Action<int[]> action)
        {
            DimensionInfo dimensionInfo = _arrayInfo.DimensionInfos[0];
            for (int index = dimensionInfo.LowerBound; index < dimensionInfo.LowerBound + dimensionInfo.Length; index++)
            {
                var result = new List<int>();

                // Adding the first coordinate
                result.Add(index);

                if (_arrayInfo.DimensionInfos.Count < 2)
                {
                    // only one dimension
                    action.Invoke(result.ToArray());
                    continue;
                }

                // further dimensions
                forEach(_arrayInfo.DimensionInfos, 1, result, action);
            }
        }


        /// <summary>
        ///   This functiona will be recursively used
        /// </summary>
        /// <param name = "dimensionInfos"></param>
        /// <param name = "dimension"></param>
        /// <param name = "coordinates"></param>
        /// <param name = "action"></param>
        private void forEach(IList<DimensionInfo> dimensionInfos, int dimension, IEnumerable<int> coordinates,
                             Action<int[]> action)
        {
            DimensionInfo dimensionInfo = dimensionInfos[dimension];
            for (int index = dimensionInfo.LowerBound; index < dimensionInfo.LowerBound + dimensionInfo.Length; index++)
            {
                var result = new List<int>(coordinates);

                // Adding the first coordinate
                result.Add(index);

                if (dimension == _arrayInfo.DimensionInfos.Count - 1)
                {
                    // This is the last dimension
                    action.Invoke(result.ToArray());
                    continue;
                }

                // Further dimensions
                forEach(_arrayInfo.DimensionInfos, dimension + 1, result, action);
            }
        }
    }

    /// <summary>
    ///   Contain info about array (i.e. how many dimensions, lower/upper bounds)
    /// </summary>
    public sealed class ArrayInfo
    {
        private IList<DimensionInfo> _dimensionInfos;

        ///<summary>
        ///</summary>
        public IList<DimensionInfo> DimensionInfos
        {
            get
            {
                if (_dimensionInfos == null) _dimensionInfos = new List<DimensionInfo>();
                return _dimensionInfos;
            }
            set { _dimensionInfos = value; }
        }
    }
}