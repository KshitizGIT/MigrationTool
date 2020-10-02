using MigrationTool.Crawler;
using System.Collections.Generic;

namespace MigrationTool.Analyzer
{
    /// <summary>
    /// Defines method to get executable scripts.
    /// </summary>
    public interface IScriptAnalyzer
    {
        /// <summary>
        /// Analyzes the <see cref="SqlFileDetail"/> file and returns executable scripts.
        /// </summary>
        /// <param name="file">The sql file to analyze.</param>
        /// <returns></returns>
        IEnumerable<string> GetScriptsToRun(SqlFileDetail file);
    }
}
