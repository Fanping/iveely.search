/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：13896622743
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.CloudComputting.CacheCommon
{
    internal class KetamaNodeLocator
    {
        /// <summary>
        /// The list of ketama node
        /// </summary>
        private SortedList<long, string> ketamaNodes;

        /// <summary>
        /// Build ketama node locator
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="nodeCopies"></param>
        public KetamaNodeLocator(List<string> nodes, int nodeCopies)
        {
            ketamaNodes = new SortedList<long, string>();
            foreach (string node in nodes)
            {
                for (int i = 0; i < nodeCopies / 4; ++i)
                {
                    byte[] digest = computeMd5(node + i);
                    for (int h = 0; h < 4; ++h)
                    {
                        long m = hash(digest, h);
                        ketamaNodes[m] = node;
                    }
                }
            }
        }

        public int GetLocatorCount()
        {
            return ketamaNodes.Count;
        }

        /// <summary>
        /// Select a node to get or set value
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public string GetNodeForKey(int hash)
        {
            string rv;
            long key = hash;
            if (!ketamaNodes.ContainsKey(key))
            {
                IList<long> keys = ketamaNodes.Keys;
                foreach (int item in keys)
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
            rv = ketamaNodes[key];
            return rv;
        }

        /// <summary>
        /// Get the hash value
        /// </summary>
        /// <param name="digest"></param>
        /// <param name="nTime"></param>
        /// <returns></returns>
        private long hash(byte[] digest, int nTime)
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
        private byte[] computeMd5(string k)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] keyBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(k));
            md5.Clear();
            return keyBytes;
        }
    }
}
