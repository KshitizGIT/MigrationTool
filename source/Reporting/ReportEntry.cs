using System;

namespace MigrationTool.Reporting
{
    /// <summary>
    ///  Class representing entry in <see cref="ExecutionReport"/>.
    /// </summary>
    public class ReportEntry
    {
        /// <summary>
        /// The modified file. 
        /// </summary>
        public string File { get; set; }
        public DateTime StartRunTime { get; set; }
        public DateTime EndRunTime { get; set; }
        public string Exception { get; set; }
        public string Status { get; set; }
        public ReportEntryType ReportEntryType { get; set; } = ReportEntryType.Info;
    }

    /// <summary>
    /// Types of Report Entry.
    /// </summary>
    public enum ReportEntryType
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }
}
