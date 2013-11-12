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

#endregion

namespace Iveely.Framework.Text.Segment
{
    /// <summary>
    /// 隐马尔科夫训练模型
    /// </summary>
    [Serializable]
    public class HMM
    {
        public State state { get; private set; }

        public Observed observed { get; private set; }

        public InitialStateProbability initialState { get; private set; }

        public Transition transition { get; private set; }

        public Complex complex;

        public HMM()
        {
            this.state = new State();
            this.observed = new Observed();
            this.initialState = new InitialStateProbability();
            this.transition = new Transition();
            this.complex = new Complex();
        }

        /// <summary>
        /// 设定HMM的状态集
        /// </summary>
        /// <param name="states"> 状态集 </param>
        public void SetState(string[] states)
        {
            foreach(var state in states)
            {
                this.state.Add(state);

                foreach(var s in states)
                {
                    this.transition.Table[state][s] = 0.0;
                }
            }
        }

        /// <summary>
        /// 添加观察序列
        /// </summary>
        /// <param name="observer"> </param>
        public void AddObserver(string observer)
        {
            this.observed.Add(observer);
        }

        protected void AddComplexProbability(string observer, string state)
        {
            object obj = this.complex.Table[observer][state];
            if(obj != null)
            {
                this.complex.Table[observer][state] = double.Parse(obj.ToString()) + 1;
            }
            else
            {
                this.complex.Table[observer][state] = 1;
            }
        }

        protected void AddTransferProbability(string from, string to, double defaultValue)
        {
            object obj = this.transition.Table[from][to];
            if(obj != null)
            {
                this.transition.Table[from][to] = double.Parse(obj.ToString()) + defaultValue;
            }
            else
            {
                this.transition.Table[from][to] = defaultValue;
            }
        }

        protected void AddInitialStateProbability(string state, double defaultValue)
        {
            this.initialState.Add(state, defaultValue);
        }

        public int[] Decode(string[] input, out double probability, int type = 1)
        {
            int inputLength = input.Length;
            int stateCount = this.state.Length;
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
                object obj = complex.Table[this.state[i]][input[0]];
                if(obj != null)
                {
                    a[i, 0] = (1.0*Math.Log(initialState[this.state[i]])) - Math.Log(double.Parse(obj.ToString()));
                }
            }

            for(int t = type; t < inputLength; t++)
            {
                for(int j = 0; j < stateCount; j++)
                {
                    minState = 0;
                    minWeight = a[0, t - type] -
                                Math.Log(double.Parse(this.transition.Table[this.state[0]][this.state[j]].ToString()));
                    //minWeight = Math.Abs(minWeight);

                    for(int i = 0; i < stateCount; i++)
                    {
                        double weight = a[i, t - type] -
                                        Math.Log(
                                            double.Parse(
                                                this.transition.Table[this.state[i]][this.state[j]].ToString()));
                        //weight = Math.Abs(weight);
                        if(weight < minWeight)
                        {
                            minState = i;
                            minWeight = weight;
                        }
                    }
                    object obj = complex.Table[this.state[j]][input[t]];
                    if(obj != null)
                    {
                        a[j, t] = minWeight - Math.Log(double.Parse(obj.ToString()));
                        s[j, t] = minState;
                    }
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