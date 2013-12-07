namespace NDatabase.Api
{
    /// <summary>
    /// Index Manager - allows to give access to class level configuration like adding an index, checking if index exists, rebuilding an index,...
    /// </summary>
    public interface IIndexManager
    {
        /// <param name="indexName"> The name of the index </param>
        /// <param name="indexFields"> The list of fields of the index. Every field needs to implement IComparable.</param>
        void AddUniqueIndexOn(string indexName, params string[] indexFields);

        /// <param name="indexName"> The name of the index </param>
        /// <param name="indexFields"> The list of fields of the index. Every field needs to implement IComparable.</param>
        void AddIndexOn(string indexName, params string[] indexFields);

        /// <summary>
        ///   To check if an index exist
        /// </summary>
        /// <param name="indexName">Existing index name</param>
        bool ExistIndex(string indexName);

        /// <summary>
        /// Rebuild existing index
        /// </summary>
        /// <param name="indexName">Existing index name</param>
        void RebuildIndex(string indexName);

        /// <summary>
        /// Delete existing index
        /// </summary>
        /// <param name="indexName">Existing index name</param>
        void DeleteIndex(string indexName);
    }
}
