using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Iveely.Framework.Network
{
    /// <summary>
    /// 下载器
    /// </summary>
    public class Downloader
    {
        public bool SyncDownload(string url, string savePath)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
                webClient.DownloadFile(url, savePath);
                if (File.Exists(savePath))
                {
                    return true;
                }
                return false;
            }
            catch (Exception exception)
            {
                Log.Logger.Error(exception);
            }
            return false;
        }

        /// <summary>
        /// 远程文件是否存在
        /// </summary>
        /// <param name="fileUrl"></param>
        /// <returns></returns>
        public bool RemoteFileExists(string fileUrl)
        {
            bool result = false;
            WebResponse response = null;
            try
            {
                WebRequest req = WebRequest.Create(fileUrl);
                response = req.GetResponse();
                result = response == null ? false : true;
            }
            catch (Exception ex)
            {
                result = false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            return result;
        }

    }
}
