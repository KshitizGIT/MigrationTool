using MigrationTool.Crawler;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MigrationTool.Analyzer
{
    /// <summary>
    /// Used to analyze the script file.
    /// </summary>
    public class ScriptAnalyzer : IScriptAnalyzer
    {
        /// <summary>
        /// Breaks the script via GO statements.
        /// </summary>
        /// <param name="file">The file to analyze.</param>
        /// <returns>List of executable scripts.</returns>
        public IEnumerable<string> GetScriptsToRun(SqlFileDetail file)
        {
            // Removing ANSI commands like GO , ANSI OFF
            // Also handles GO inside single quotes.
            var matches = Regex.Split(file.Content, @"\b[Gg][oO]\b");
            foreach (var match in matches)
            {
                if (string.IsNullOrWhiteSpace(match))
                    continue; 

                yield return match;
            }
        }
    }
}
