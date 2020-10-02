using System;
using System.Collections.Generic;
using System.IO;

namespace MigrationTool.Crawler
{
    /// <summary>
    /// Retrives a single sql file.
    /// </summary>
    public class SingleFileLister : SqlFilesLister
    {
        /// <summary>
        /// Creates a new instance of <see cref="SingleFileLister"/>.
        /// </summary>
        /// <param name="file"></param>
        public SingleFileLister(string file) : base(file)
        {
            if (!File.Exists(file))
            {
                throw new Exception("The provided file doesnot exists.");
            }
        }

        /// <summary>
        /// Get the sql file with its content.
        /// </summary>
        /// <returns>The sql file with its content.</returns>
        public override IEnumerable<SqlFileDetail> GetSqlFilesWithContents()
        {
            return new List<SqlFileDetail> {
                new SqlFileDetail{
                FullPath = FilesLocation,
                Name = Path.GetFileName(FilesLocation),
                Content = File.ReadAllText(FilesLocation),
                HashSum  = HashSumAnalyzer.GetFileHash(FilesLocation)
                }
            };
        }
    }
}
