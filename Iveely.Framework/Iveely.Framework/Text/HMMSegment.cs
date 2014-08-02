using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iveely.Framework.Text
{
    [Serializable]
    public class HMMSegment
    {
        private const int K = 101;

        private string[] rtrans = new string[K + 1];

        int[] cnt = new int[K];

        int[,] ccnt = new int[K, K];

        double[,] p = new double[K + 1, K + 1];

        private SortedDictionary<string, int> trans = new SortedDictionary<string, int>();

        private Tnode root = new Tnode();

        private static HMMSegment segment;

        //trie树节点类
        [Serializable]
        public class Tnode
        {
            public const int k = 102;
            public SortedDictionary<char, Tnode> mp = new SortedDictionary<char, Tnode>();
            public bool ed;
            public int sum;
            public int[] each = new int[k];
            public Tnode()
            {
                ed = false;
                mp.Clear();
                Array.Clear(each, 0, each.Length);
                sum = 0;
            }
        }

        //DP时状态表示类
        [Serializable]
        public class Tans
        {
            public string str = "";
            public const int k = 102;
            public double[] each = new double[k];
            public int[] last = new int[k];
        }

        private HMMSegment()
        {
            Learn();
        }

        public static HMMSegment GetInstance()
        {
            if (segment == null)
            {
                segment = new HMMSegment();
            }
            return segment;
        }

        private void Insert(string tmp, int sp)
        {
            Tnode now = root;
            int ptr = 0;
            while (ptr < tmp.Length)
            {
                char t = tmp[ptr];
                if (!now.mp.ContainsKey(t))
                {
                    now.mp[t] = new Tnode();
                }
                now = now.mp[t];
                ptr++;
            }
            now.ed = true;
            now.sum++;
            now.each[sp]++;
        }

        //制作词性和编号之间的对应关系

        private void InitTrans()
        {
            trans["nz"] = 0;
            rtrans[0] = "nz";
            trans["t"] = 1;
            rtrans[1] = "t";
            trans["ng"] = 2;
            rtrans[2] = "ng";
            trans["p"] = 3;
            rtrans[3] = "p";
            trans["w"] = 4;
            rtrans[4] = "w";
            trans["nsf"] = 5;
            rtrans[5] = "nsf";
            trans["v"] = 6;
            rtrans[6] = "v";
            trans["n"] = 7;
            rtrans[7] = "n";
            trans["ude1"] = 8;
            rtrans[8] = "ude1";
            trans["f"] = 9;
            rtrans[9] = "f";
            trans["mq"] = 10;
            rtrans[10] = "mq";
            trans["vn"] = 11;
            rtrans[11] = "vn";
            trans["cc"] = 12;
            rtrans[12] = "cc";
            trans["uyy"] = 13;
            rtrans[13] = "uyy";
            trans["d"] = 14;
            rtrans[14] = "d";
            trans["a"] = 15;
            rtrans[15] = "a";
            trans["c"] = 16;
            rtrans[16] = "c";
            trans["rz"] = 17;
            rtrans[17] = "rz";
            trans["qt"] = 18;
            rtrans[18] = "qt";
            trans["m"] = 19;
            rtrans[19] = "m";
            trans["vi"] = 20;
            rtrans[20] = "vi";
            trans["gm"] = 21;
            rtrans[21] = "gm";
            trans["ntc"] = 22;
            rtrans[22] = "ntc";
            trans["nrf"] = 23;
            rtrans[23] = "nrf";
            trans["uls"] = 24;
            rtrans[24] = "uls";
            trans["nis"] = 25;
            rtrans[25] = "nis";
            trans["x"] = 26;
            rtrans[26] = "x";
            trans["vshi"] = 27;
            rtrans[27] = "vshi";
            trans["rzv"] = 28;
            rtrans[28] = "rzv";
            trans["uzhe"] = 29;
            rtrans[29] = "uzhe";
            trans["nnd"] = 30;
            rtrans[30] = "nnd";
            trans["ude3"] = 31;
            rtrans[31] = "ude3";
            trans["ns"] = 32;
            rtrans[32] = "ns";
            trans["nnt"] = 33;
            rtrans[33] = "nnt";
            trans["nr"] = 34;
            rtrans[34] = "nr";
            trans["pbei"] = 35;
            rtrans[35] = "pbei";
            trans["ule"] = 36;
            rtrans[36] = "ule";
            trans["vf"] = 37;
            rtrans[37] = "vf";
            trans["ad"] = 38;
            rtrans[38] = "ad";
            trans["s"] = 39;
            rtrans[39] = "s";
            trans["pba"] = 40;
            rtrans[40] = "pba";
            trans["an"] = 41;
            rtrans[41] = "an";
            trans["qv"] = 42;
            rtrans[42] = "qv";
            trans["udeng"] = 43;
            rtrans[43] = "udeng";
            trans["b"] = 44;
            rtrans[44] = "b";
            trans["k"] = 45;
            rtrans[45] = "k";
            trans["vx"] = 46;
            rtrans[46] = "vx";
            trans["rr"] = 47;
            rtrans[47] = "rr";
            trans["vl"] = 48;
            rtrans[48] = "vl";
            trans["vyou"] = 49;
            rtrans[49] = "vyou";
            trans["ry"] = 50;
            rtrans[50] = "ry";
            trans["vd"] = 51;
            rtrans[51] = "vd";
            trans["z"] = 52;
            rtrans[52] = "z";
            trans["bl"] = 53;
            rtrans[53] = "bl";
            trans["nt"] = 54;
            rtrans[54] = "nt";
            trans["uzhi"] = 55;
            rtrans[55] = "uzhi";
            trans["vg"] = 56;
            rtrans[56] = "vg";
            trans["r"] = 57;
            rtrans[57] = "r";
            trans["q"] = 58;
            rtrans[58] = "q";
            trans["nf"] = 59;
            rtrans[59] = "nf";
            trans["al"] = 60;
            rtrans[60] = "al";
            trans["rzs"] = 61;
            rtrans[61] = "rzs";
            trans["uguo"] = 62;
            rtrans[62] = "uguo";
            trans["dl"] = 63;
            rtrans[63] = "dl";
            trans["nto"] = 64;
            rtrans[64] = "nto";
            trans["gi"] = 65;
            rtrans[65] = "gi";
            trans["usuo"] = 66;
            rtrans[66] = "usuo";
            trans["ude2"] = 67;
            rtrans[67] = "ude2";
            trans["gg"] = 68;
            rtrans[68] = "gg";
            trans["nrj"] = 69;
            rtrans[69] = "nrj";
            trans["ag"] = 70;
            rtrans[70] = "ag";
            trans["tg"] = 71;
            rtrans[71] = "tg";
            trans["ulian"] = 72;
            rtrans[72] = "ulian";
            trans["dg"] = 73;
            rtrans[73] = "dg";
            trans["y"] = 74;
            rtrans[74] = "y";
            trans["ryv"] = 75;
            rtrans[75] = "ryv";
            trans["nhd"] = 76;
            rtrans[76] = "nhd";
            trans["gb"] = 77;
            rtrans[77] = "gb";
            trans["j"] = 78;
            rtrans[78] = "j";
            trans["rys"] = 79;
            rtrans[79] = "rys";
            trans["ntu"] = 80;
            rtrans[80] = "ntu";
            trans["l"] = 81;
            rtrans[81] = "l";
            trans["nit"] = 82;
            rtrans[82] = "nit";
            trans["o"] = 83;
            rtrans[83] = "o";
            trans["nmc"] = 84;
            rtrans[84] = "nmc";
            trans["u"] = 85;
            rtrans[85] = "u";
            trans["gp"] = 86;
            rtrans[86] = "gp";
            trans["ryt"] = 87;
            rtrans[87] = "ryt";
            trans["nhm"] = 88;
            rtrans[88] = "nhm";
            trans["udh"] = 89;
            rtrans[89] = "udh";
            trans["rzt"] = 90;
            rtrans[90] = "rzt";
            trans["Rg"] = 91;
            rtrans[91] = "Rg";
            trans["gc"] = 92;
            rtrans[92] = "gc";
            trans["e"] = 93;
            rtrans[93] = "e";
            trans["Mg"] = 94;
            rtrans[94] = "Mg";
            trans["ntcb"] = 95;
            rtrans[95] = "ntcb";
            trans["i"] = 96;
            rtrans[96] = "i";
            trans["nba"] = 97;
            rtrans[97] = "nba";
            trans["na"] = 98;
            rtrans[98] = "na";
            trans["nbc"] = 99;
            rtrans[99] = "nbc";
            trans["nr1"] = 100;
            rtrans[100] = "nr1";
            trans["nr2"] = 101;
            rtrans[101] = "nr2";

        }

        private void Learn()
        {
            GetTrie(@"Init\Split_Corpus.txt");
        }

        private void GetTrie(string filePath)
        {
            InitTrans();
            Array.Clear(cnt, 0, cnt.Length);
            Array.Clear(ccnt, 0, ccnt.Length);
            StreamReader dict = new StreamReader(filePath, Encoding.UTF8);
            string cat;
            while (!dict.EndOfStream)
            {
                cat = dict.ReadLine();
                cat = cat.Replace("  ", " ");
                cat = cat.Replace("  ", " ");
                cat = cat.Replace("  ", " ");
                string[] wds = cat.Replace(" ", "@").Split('@');
                int last = -1;
                foreach (string wd in wds)
                {
                    if (wd.Length == 0) break;
                    string[] t = wd.Split('/');
                    if (t.Length == 2)
                    {
                        try
                        {
                            int now = trans[t[1].Replace("]", "").Replace("[", "")];
                            Insert(t[0], now);
                            cnt[now]++;
                            if (last != -1)
                            {
                                ccnt[last, now]++;
                            }
                            last = now;
                        }
                        catch
                        {

                        }

                    }
                }
            }
            for (int i = 0; i < K; ++i)
            {
                for (int j = 0; j < K; ++j)
                {
                    p[i, j] = ccnt[i, j] * 1.0 / cnt[j];
                }
            }
            for (int i = 0; i <= K; ++i)
            {
                p[K, i] = p[i, K] = 1.0;
            }
        }

        //分词，同时标注词性
        public Tuple<string[], string[]> SplitToArray(string text)
        {
            string now = text;
            if (now.Length == 0) return null;
            int ptr = 0;
            Tans[] ans = new Tans[10005];
            int s = 0;
            while (ptr < now.Length)
            {
                if (now[ptr] == '\r' && now[ptr + 1] == '\n')
                {
                    ptr += 2;
                    continue;
                }
                ans[s] = new Tans();
                int deep = 0;
                Tnode trie = root;
                for (int i = 0; i + ptr < now.Length; ++i)
                {
                    char t = now[i + ptr];
                    if (!trie.mp.ContainsKey(t)) break;
                    trie = trie.mp[t];
                    if (trie.ed)
                    {
                        deep = i + 1;
                        ans[s].str = "";
                        for (int j = 0; j < deep; ++j)
                        {
                            ans[s].str += now[ptr + j];
                        }
                        if (s == 0)
                        {
                            for (int j = 0; j <= K; ++j)
                            {
                                if (trie.sum > 0) ans[s].each[j] = trie.each[j] * 1.0 / trie.sum;
                                else ans[s].each[j] = 0;
                                ans[s].last[j] = -1;
                            }
                        }
                        else
                        {
                            for (int j = 0; j < K; ++j)
                            {
                                int save = -1;
                                double big = -1;
                                for (int r = 0; r <= K; ++r)
                                {
                                    if (ans[s - 1].each[r] * p[r, j] > big)
                                    {
                                        big = ans[s - 1].each[r] * p[r, j];
                                        save = r;
                                    }
                                }
                                ans[s].each[j] = big * trie.each[j] * 1.0 / trie.sum;
                                ans[s].last[j] = save;
                            }
                            ans[s].each[K] = 0;
                        }
                    }
                }
                if (deep == 0)
                {
                    deep++;
                    for (int j = 0; j < K; ++j) ans[s].each[j] = 0;
                    ans[s].each[K] = 1;
                    ans[s].str = "";
                    for (int j = 0; j < deep; ++j)
                    {
                        ans[s].str += now[ptr + j];
                    }
                    if (s == 0)
                    {
                        ans[s].last[K] = -1;
                    }
                    else
                    {
                        int save = -1;
                        double big = -1;
                        for (int r = 0; r <= K; ++r)
                        {
                            if (ans[s - 1].each[r] > big)
                            {
                                big = ans[s - 1].each[r];
                                save = r;
                            }
                        }
                        ans[s].last[K] = save;
                    }
                }
                ptr += deep;
                s++;
            }
            int[] optlist = new int[10005];
            double bb = -1;
            for (int i = 0; i <= K; ++i)
            {
                if (ans[s - 1].each[i] > bb)
                {
                    optlist[s - 1] = i;
                    bb = ans[s - 1].each[i];
                }
            }
            for (int i = s - 1; i >= 1; --i)
            {
                int da, db;
                da = optlist[i];
                db = ans[i].last[da];
                optlist[i - 1] = db;
            }
            List<string> words = new List<string>();
            List<string> semantics = new List<string>();
            for (int i = 0; i < s; ++i)
            {
                words.Add(ans[i].str);
                semantics.Add(rtrans[optlist[i]]);
            }
            return new Tuple<string[], string[]>(words.ToArray(), semantics.ToArray());
        }

        public string SplitWithSemantic(string text, string delimeter=" ")
        {
            Tuple<string[], string[]> tuple = SplitToArray(text);
            string result = "";
            for (int i = 0; i < tuple.Item1.Length; i++)
            {
                result += tuple.Item1[i] + "/" + tuple.Item2[i] + delimeter;
            }
            return result;
        }

        public string Split(string text, string delimeter)
        {
            Tuple<string[], string[]> tuple = SplitToArray(text);
            string result = "";
            for (int i = 0; i < tuple.Item1.Length; i++)
            {
                result += tuple.Item1[i] + delimeter;
            }
            return result;
        }

        public string[] Split(string text)
        {
            Tuple<string[], string[]> tuple = SplitToArray(text);
            return tuple.Item1;
        }
    }
}
