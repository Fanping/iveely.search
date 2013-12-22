/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *世界上最快乐的事情，莫过于为理想而奋斗！
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Iveely.CloudComputing.CacheCommon
{
    internal class KetamaNodeLocator
    {
        /// <summary>
        /// The list of ketama node
        /// </summary>
        private readonly SortedList<long, string> _ketamaNodes;

        /// <summary>
        /// Build ketama node locator
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="nodeCopies"></param>
        public KetamaNodeLocator(IEnumerable<string> nodes, int nodeCopies)
        {
            _ketamaNodes = new SortedList<long, string>();
            foreach (string node in nodes)
            {
                for (int i = 0; i < nodeCopies / 4; ++i)
                {
                    byte[] digest = ComputeMd5(node + i);
                    for (int h = 0; h < 4; ++h)
                    {
                        long m = Hash(digest, h);
                        _ketamaNodes[m] = node;
                    }
                }
            }
        }

        public int GetLocatorCount()
        {
            return _ketamaNodes.Count;
        }

        /// <summary>
        /// Select a node to get or set value
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public string GetNodeForKey(int hash)
        {
            long key = hash;
            if (!_ketamaNodes.ContainsKey(key))
            {
                IList<long> keys = _ketamaNodes.Keys;
                foreach (long item in keys)
                {
                    if (item > hash)
                    {
                        key = item;
                        break;
                    }
                }
                if (key == hash)
                {
                    key = keys[0];
                }
            }
            string rv = _ketamaNodes[key];
            return rv;
        }

        /// <summary>
        /// Get the hash value
        /// </summary>
        /// <param name="digest"></param>
        /// <param name="nTime"></param>
        /// <returns></returns>
        private static long Hash(byte[] digest, int nTime)
        {
            long rv = ((long)(digest[3 + nTime * 4] & 0xFF) << 24)
                    | ((long)(digest[2 + nTime * 4] & 0xFF) << 16)
                    | ((long)(digest[1 + nTime * 4] & 0xFF) << 8)
                    | ((long)digest[0 + nTime * 4] & 0xFF);
            return rv & 0xffffffffL;
        }

        /// <summary>
        /// Get MD5
        /// </summary>
        private static byte[] ComputeMd5(string k)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(k));
            md5.Clear();
            return keyBytes;
        }
    }
}
