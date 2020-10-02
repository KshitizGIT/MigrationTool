using System;
using System.Collections.Generic;
using System.IO;

namespace MigrationTool.Crawler
{
    /// <summary>
    /// List all sql files inside the specified directory.
    /// </summary>
    public class DirectoryFilesLister : SqlFilesLister
    {
        public DirectoryFilesLister(string directory) : base(directory)
        {
            if (!Directory.Exists(directory))
                throw new Exception("The provided directory doesnot exists. Please provide existing directory path location");
        }

        public override IEnumerable<SqlFileDetail> GetSqlFilesWithContents()
        {
            foreach (var file in Directory.EnumerateFiles(FilesLocation, "*.sql"))
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
