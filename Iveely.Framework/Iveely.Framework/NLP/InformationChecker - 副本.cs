/*==========================================
 *创建人：刘凡平
 *邮  箱：liufanping@iveely.com
 *电  话：
 *版  本：0.1.0
 *Iveely=I void everything,except love you!
 *========================================*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.NLP
{
    /// <summary>
    /// 信息检查
    /// </summary>
    public class InformationChecker
    {
        private static InformationExtracter _extracter;

        public InformationChecker()
        {
            if (_extracter == null)
            {
                _extracter = InformationExtracter.GetInstance();
            }
        }

        /// <summary>
        /// 根据问题获得答案
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public string GetAnswer(string question)
        {
            List<string[]> results = QuestionEntityExtracter.GetInstance().GetInforByPattern(question);

            //如果提取到信息
            if (results.Count > 0)
            {
                foreach (var stringse in results)
                {
                    List<string> relations = _extracter.GetRelation(stringse[0], stringse[1], stringse[2]);
                    if (relations != null)
                    {
                        return string.Join("\r\n", relations);
                    }
                }
            }
            return string.Empty;
        }
    }
}
