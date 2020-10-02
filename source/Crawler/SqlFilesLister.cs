using System.Collections.Generic;

namespace MigrationTool.Crawler
{
    /// <summary>
    /// A base sql files retriever from the specified directory.
    /// </summary>
    public abstract class SqlFilesLister : ISqlFilesLister
    {
        private readonly string _directory;
        /// <summary>
        /// Creates a new instance of SqlFilesLister
        /// </summary>
        /// <param name="directory"></param>
        public SqlFilesLister(string directory)
        {
            _directory = directory;
        }

        /// <summary>
        /// The path of the sql files.
        /// </summary>
        public string FilesLocation
        {
            get { return _directory; }
        }

        /// <summary>
        /// Retrieves list of sql files with their contents. <see cref="SqlFileDetail"/>
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<SqlFileDetail> GetSqlFilesWithContents();
    }
}
