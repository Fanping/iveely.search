using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Iveely.STSdb4.General.IO
{
    public static class IOUtils
    {
        public static long GetTotalFreeSpace(string driveName)
        {
            driveName = driveName.ToUpper();

            var drive = DriveInfo.GetDrives().Where(x => x.IsReady && x.Name == driveName).FirstOrDefault();

            return drive != null ? drive.TotalFreeSpace : -1;
        }

        public static long GetTotalSpace(string driveName)
        {
            driveName = driveName.ToUpper();

            var drive = new DriveInfo(driveName);

            return drive != null ? drive.TotalSize : -1;
        }
    }
}
