using System;
using System.Collections.Generic;
using System.IO;

namespace MigrationTool.Crawler
{
    /// <summary>
    /// Recursively retrives sql file from folder and all its subfolders.
    /// </summary>
    public class DirectoryRecursiveLister : SqlFilesLister
    {
        public DirectoryRecursiveLister(string directory) : base(directory)
        {
            if (!Directory.Exists(directory))
                throw new Exception("The provided directory doesnot exists. Please provide existing directory path location");
        }

        /// <summary>
        /// Retrives sql files with its contents.
        /// </summary>
        /// <returns>The list of sql files with its content.</returns>
        public override IEnumerable<SqlFileDetail> GetSqlFilesWithContents()
        {
            foreach (var file in Directory.EnumerateFiles(FilesLocation, "*.sql", SearchOption.AllDirectories))
            {
                SqlFileDetail sqlFile = null;
                try
                {
                    sqlFile = new SqlFileDetail()
                    {
                        FullPath = file,
                        Name = Path.GetFileName(file),
                        Content = File.ReadAllText(file),
                        HashSum = HashSumAnalyzer.GetFileHash(file)
                    };
                }
                catch (Exception)
                {
                    throw;
                }
                yield return sqlFile;
            }
        }
    }
}
