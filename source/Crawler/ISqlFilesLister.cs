using System.Collections.Generic;

namespace MigrationTool.Crawler
{
    /// <summary>
    /// Defines methods for retrieving sql file.
    /// </summary>
    public interface ISqlFilesLister
    {
        /// <summary>
        /// Get Sql Files with content
        /// </summary>
        /// <returns></returns>
        IEnumerable<SqlFileDetail> GetSqlFilesWithContents();

        /// <summary>
        /// The path of the sql file.
        /// </summary>
        string FilesLocation { get; }
    }
}
