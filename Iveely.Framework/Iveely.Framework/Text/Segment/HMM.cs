// ------------------------------------------------------------------------------------------------
//  <copyright file="HMM.cs" company="Iveely">
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
using System.Globalization;

#endregion

namespace Iveely.Framework.Text.Segment
{
    /// <summary>
    /// 隐马尔科夫训练模型
    /// </summary>
    [Serializable]
    public class HMM
    {
        public State State { get; private set; }

        public Observed Observed { get; private set; }

        public InitialStateProbability InitialState { get; private set; }

        public Transition Transition { get; private set; }

        public Complex Complex;

        public HMM()
        {
            State = new State();
            Observed = new Observed();
            InitialState = new InitialStateProbability();
            Transition = new Transition();
            Complex = new Complex();
        }

        /// <summary>
        /// 设定HMM的状态集
        /// </summary>
        /// <param name="states"> 状态集 </param>
        public void SetState(string[] states)
        {
            foreach(var state in states)
            {
                State.Add(state);

                foreach(var s in states)
                {
                    Transition.Table[state][s] = 0.0;
                }
            }
        }

        /// <summary>
        /// 添加观察序列
        /// </summary>
        /// <param name="observer"> </param>
        public void AddObserver(string observer)
        {
            Observed.Add(observer);
        }

        protected void AddComplexProbability(string observer, string state)
        {
            object obj = Complex.Table[observer][state];
            Complex.Table[observer][state] = double.Parse(obj.ToString()) + 1;
        }

        protected void AddTransferProbability(string from, string to, double defaultValue)
        {
            object obj = Transition.Table[from][to];
            Transition.Table[@from][to] = double.Parse(obj.ToString()) + defaultValue;
        }

        protected void AddInitialStateProbability(string state, double defaultValue)
        {
            InitialState.Add(state, defaultValue);
        }

        public int[] Decode(string[] input, out double probability, int type = 1)
        {
            int inputLength = input.Length;
            int stateCount = State.Length;
            int minState;
            double minWeight;


            int[,] s = new int[stateCount,inputLength];


            double[,] a = new double[stateCount,inputLength];

            //初始化
            for(int i = 0; i < stateCount; i++)
            {
                for(int j = 0; j < inputLength; j++)
                {
                    s[i, j] = type - 1;
                    a[i, j] = 0.0;
                }
            }

            //每一个状态对应第一个输入的距离
            for(int i = 0; i < stateCount; i++)
            {
                object obj = Complex.Table[State[i]][input[0]];
                if(obj != null)
                {
                    a[i, 0] = (1.0*Math.Log(InitialState[State[i]])) - Math.Log(double.Parse(obj.ToString()));
                }
            }

            for(int t = type; t < inputLength; t++)
            {
                for(int j = 0; j < stateCount; j++)
                {
                    minState = 0;
                    minWeight = a[0, t - type] -
                                Math.Log(double.Parse(Transition.Table[State[0]][State[j]].ToString(CultureInfo.InvariantCulture)));
                    //minWeight = Math.Abs(minWeight);

                    for(int i = 0; i < stateCount; i++)
                    {
                        double weight = a[i, t - type] -
                                        Math.Log(
                                            double.Parse(
                                                Transition.Table[State[i]][State[j]].ToString(CultureInfo.InvariantCulture)));
                        //weight = Math.Abs(weight);
                        if(weight < minWeight)
                        {
                            minState = i;
                            minWeight = weight;
                        }
                    }
                    object obj = Complex.Table[State[j]][input[t]];
                    a[j, t] = minWeight - Math.Log(double.Parse(obj.ToString()));
                    s[j, t] = minState;
                }
            }
            minState = 0;
            minWeight = a[0, inputLength - 1];
            for(int i = 1; i < stateCount; i++)
            {
                if(a[i, inputLength - 1] < minWeight)
                {
                    minState = i;
                    minWeight = a[i, inputLength - 1];
                }
            }

            int[] path = new int[input.Length];
            //分词
            if(type == 1)
            {
                path[inputLength - 1] = minState;
                for(int m = inputLength - 2; m >= 0; m--)
                {
                    path[m] = s[path[m + 1], m + 1];
                }
            }
                //词性分析
            else if(type == 0)
            {
                for(int i = 0; i < inputLength; i++)
                {
                    int max = -1;
                    for(int j = 0; j < stateCount; j++)
                    {
                        if(max <= s[j, i])
                        {
                            max = s[j, i];
                            path[i] = j;
                        }
                    }
                    if(max == -1)
                    {
                        path[i] = 0;
                    }
                }
            }
            probability = Math.Exp(-minWeight);
            return path;
        }
    }
}