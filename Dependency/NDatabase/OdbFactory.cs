using System;
using System.IO;
using System.Threading;
using NDatabase.Api;
using NDatabase.Container;
using NDatabase.Core;
using NDatabase.Core.Engine;
using NDatabase.Core.Query;
using NDatabase.Core.Session;
using NDatabase.Meta;
using NDatabase.Services;
using NDatabase.Transaction;

namespace NDatabase
{
    /// <summary>
    /// The NDatabase Factory to open new instance of local odb.
    /// </summary>
    public static class OdbFactory
    {
        [ThreadStatic]
        private static string _last;

        static OdbFactory()
        {
            DependencyContainer.Register<IMetaModelCompabilityChecker>(() => new MetaModelCompabilityChecker());
            DependencyContainer.Register<IQueryManager>(() => new QueryManager());

            DependencyContainer.Register<IOdbForTrigger>((storageEngine) => new Odb((IStorageEngine)storageEngine));

            DependencyContainer.Register<IReflectionService>(() => new ReflectionService());

            DependencyContainer.Register<IObjectWriter>((storageEngine) => new ObjectWriter((IStorageEngine)storageEngine));
            DependencyContainer.Register<IObjectReader>((storageEngine) => new ObjectReader((IStorageEngine)storageEngine));

            DependencyContainer.Register<ISession>((storageEngine) => new LocalSession((IStorageEngine)storageEngine));
        }

        /// <summary>
        /// Opens the database instance with the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>IOdb.</returns>
        public static IOdb Open(string fileName)
        {
            Monitor.Enter(string.Intern(Path.GetFullPath(fileName)));
            
            _last = fileName;
            return Odb.GetInstance(fileName);
        }

        /// <summary>
        /// Opens the database instance with the last given name.
        /// </summary>
        /// <returns>IOdb.</returns>
        public static IOdb OpenLast()
        {
            return Open(_last);
        }

        /// <summary>
        /// Opens a database in the In-Memory mode.
        /// </summary>
        /// <returns>IOdb implementation.</returns>
        public static IOdb OpenInMemory()
        {
            return Odb.GetInMemoryInstance();
        }

        /// <summary>
        /// Deletes the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public static void Delete(string fileName)
        {
            lock (string.Intern(Path.GetFullPath(fileName)))
            {
                if (!File.Exists(fileName))
                    return;

                File.Delete(fileName);
            }
        }
    }
}
