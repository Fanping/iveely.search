using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using NDatabase.Exceptions;

namespace NDatabase.IO
{
    internal static class OdbFileManager
    {
        private const int DefaultBufferSize = 4096 * 2;
        private const int NumberOfTries = 60;
        private const int TimeIntervalBetweenTries = 1000;

        internal static FileStream GetStream(string wholeFileName)
        {
            var tries = 0;

            while (true)
            {
                try
                {
                    return new FileStream(wholeFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None,
                                          DefaultBufferSize, FileOptions.RandomAccess);
                }
                catch (IOException e)
                {
                    if (!IsFileLocked(e))
                        throw;

                    if (++tries > NumberOfTries)
                        throw new OdbRuntimeException(NDatabaseError.FileNotFoundOrItIsAlreadyUsed.AddParameter("The file is locked too long: " + e.Message), e);

                    Thread.Sleep(TimeIntervalBetweenTries);
                }
            }
        }

        private static bool IsFileLocked(Exception exception)
        {
            var errorCode = Marshal.GetHRForException(exception) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33;
        }
    }
}