using System.IO;

namespace NDatabase.IO
{
    /// <summary>
    ///   Database Parameters for local database access
    /// </summary>
    internal sealed class FileIdentification : IDbIdentification
    {
        private readonly string _fileName;

        internal FileIdentification(string name)
        {
            _fileName = name;
        }

        #region IFileIdentification Members

        public string Directory
        {
            get
            {
                var fullPath = Path.GetFullPath(_fileName);
                return Path.GetDirectoryName(fullPath);
            }
        }

        public string Id
        {
            get { return GetCleanFileName(); }
        }

        public bool IsNew()
        {
            return !File.Exists(_fileName);
        }

        public void EnsureDirectories()
        {
            OdbDirectory.Mkdirs(FileName);
        }

        public IMultiBufferedFileIO GetIO(int bufferSize)
        {
            return new MultiBufferedFileIO(FileName, bufferSize);
        }

        public IDbIdentification GetTransactionIdentification(long creationDateTime, string sessionId)
        {
            var filename =
                string.Format("{0}-{1}-{2}.transaction", Id, creationDateTime, sessionId);

            return new InMemoryIdentification(filename);
        }

        public string FileName
        {
            get { return _fileName; }
        }

        #endregion

        public override string ToString()
        {
            return _fileName;
        }

        private string GetCleanFileName()
        {
            return Path.GetFileName(_fileName);
        }
    }
}