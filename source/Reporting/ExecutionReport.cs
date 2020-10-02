using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace MigrationTool.Reporting
{
    /// <summary>
    /// A comman signature invoked when execution report changes.
    /// </summary>
    /// <param name="sender">The object that changes the Execution Report.</param>
    /// <param name="args">Execution Report changes information.</param>
    public delegate void ExecutionReportModified(object sender, ExecutionReportModifiedArgs[] args);


    /// <summary>
    /// The execution report created by the <see cref="Migrator"/>
    /// Contains information regarding success or failure the executed scripts.
    /// </summary>
    public class ExecutionReport
    {
        private ConcurrentQueue<ReportEntry> _entries = new ConcurrentQueue<ReportEntry>();

        /// <summary>
        /// The event fired when a Execution Report changes.
        /// </summary>
        public event ExecutionReportModified Modified;

        /// <summary>
        /// Adds a entry to the execution report.
        /// </summary>
        /// <param name="report">The execution report to add entry to <see cref="ExecutionReport>."/></param>
        /// <param name="entry">The entry to add to the execution report <see cref="ReportEntry"/>.</param>
        public static void AddEntry(ExecutionReport report, ReportEntry entry)
        {
            report.AddEntry(entry);
        }

        /// <summary>
        /// Adds a entry.
        /// </summary>
        /// <param name="entry">The entry to add to the execution report <see cref="ReportEntry"/>.</param>
        public void AddEntry(ReportEntry entry) 
        {
            _entries.Enqueue(entry);
            RaiseModified($"Completed processing script '{entry.File}'", entry);
        } 

       #region Write Report Segments

        /// <summary>
        /// Writes the details in the execution report to the a xml file asynchronoulsy.
        /// </summary>
        /// <param name="filePath"></param>
        public async Task WriteReportAsync(string filePath)
        {
            using (var xmlWriter = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true, Async = true }))
            {
                const string piText = "type='text/xsl' href='MigrationReportViewer.xsl'";
                await xmlWriter.WriteStartDocumentAsync();
                await xmlWriter.WriteProcessingInstructionAsync("xml-stylesheet", piText);
                await xmlWriter.WriteStartElementAsync(string.Empty,"MigrationReport",string.Empty);
                await WriteMigrationConfigurationAsync(xmlWriter);
                await WriteCounterStatsAsync(xmlWriter);
                await xmlWriter.WriteStartElementAsync(string.Empty,"Entries",string.Empty);
                while (_entries.Count > 0)
                {
                    ReportEntry entry;
                    _entries.TryDequeue(out entry);
                    if (entry != null)
                    {
                        await xmlWriter.WriteStartElementAsync(string.Empty, "Entry", string.Empty);
                        await xmlWriter.WriteAttributeStringAsync(string.Empty,"Type", string.Empty,entry.ReportEntryType.ToString());
                        await xmlWriter.WriteAttributeStringAsync(string.Empty,"Status",string.Empty, entry.Status);
                        await xmlWriter.WriteElementStringAsync(string.Empty,"Name",string.Empty, entry.File);
                        await xmlWriter.WriteElementStringAsync(string.Empty,"StartTime",string.Empty, entry.StartRunTime.ToString());
                        await xmlWriter.WriteElementStringAsync(string.Empty,"EndTime",string.Empty, entry.EndRunTime.ToString());
                        await xmlWriter.WriteElementStringAsync(string.Empty,"Exception",string.Empty,
                            string.IsNullOrEmpty(entry.Exception) ? entry.Exception : $"Exception: {entry.Exception}");
                        await xmlWriter.WriteEndElementAsync();
                    }
                }
                //End Entries Nodes
                await xmlWriter.WriteEndElementAsync();
                //End MigrationReports Node
                await xmlWriter.WriteEndElementAsync();
                await xmlWriter.WriteEndDocumentAsync();
            }
        }

        /// <summary>
        /// Writes the details in the execution report to a xml file.
        /// </summary>
        /// <param name="filePath"></param>
        public void WriteReport(string filePath)
        {
            using (var xmlWriter = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true }))
            {
                xmlWriter.WriteStartDocument();
                const string piText = "type='text/xsl' href='MigrationReportViewer.xsl'";
                xmlWriter.WriteProcessingInstruction("xml-stylesheet", piText);
                xmlWriter.WriteStartElement("MigrationReport");
                WriteMigrationConfiguration(xmlWriter);
                WriteCounterStats(xmlWriter);
                xmlWriter.WriteStartElement("Entries");
                while (_entries.Count > 0)
                {
                    ReportEntry entry;
                    _entries.TryDequeue(out entry);
                    if (entry != null)
                    {
                        xmlWriter.WriteStartElement("Entry");
                        xmlWriter.WriteAttributeString("Type", entry.ReportEntryType.ToString());
                        xmlWriter.WriteAttributeString("Status", entry.Status);
                        xmlWriter.WriteElementString("Name", entry.File);
                        xmlWriter.WriteElementString("StartTime", entry.StartRunTime.ToString());
                        xmlWriter.WriteElementString("EndTime", entry.EndRunTime.ToString());
                        xmlWriter.WriteElementString("Exception",
                            string.IsNullOrEmpty(entry.Exception) ? entry.Exception : $"Exception: {entry.Exception}");
                        xmlWriter.WriteEndElement();
                    }
                }
                //End Entries Node
                xmlWriter.WriteEndElement();
                //End MigrationReports Node
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
        }
        /// <summary>
        /// Writes the statistics of the execution report using <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The writer object.</param>
        private void WriteCounterStats(XmlWriter writer)
        {
            writer.WriteStartElement("CounterStatistics");
            writer.WriteAttributeString("TotalEntries", _entries.Count.ToString());
            writer.WriteAttributeString("FailedEntries", _entries.Count(e => e.Status == "Fail").ToString());
            writer.WriteAttributeString("SuccessEntries", _entries.Count(e => e.Status == "Success").ToString());
            writer.WriteAttributeString("SkippedEntries", _entries.Count(e => e.Status == "Skipped").ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the statistics of the execution report using <see cref="XmlWriter"/> asynchrously.
        /// </summary>
        /// <param name="writer">The writer object.</param>
        private async Task WriteCounterStatsAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync(string.Empty, "CounterStatistics", string.Empty);
            await writer.WriteAttributeStringAsync(string.Empty, "TotalEntries", string.Empty, _entries.Count.ToString());
            await writer.WriteAttributeStringAsync(string.Empty, "FailedEntries", string.Empty, _entries.Count(e => e.Status == "Fail").ToString());
            await writer.WriteAttributeStringAsync(string.Empty, "SuccessEntries", string.Empty, _entries.Count(e => e.Status == "Success").ToString());
            await writer.WriteAttributeStringAsync(string.Empty, "SkippedEntries", string.Empty, _entries.Count(e => e.Status == "Skipped").ToString());
            await writer.WriteEndElementAsync();
        }

        /// <summary>
        /// Writes the configuration of the Migration Tool report using <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="writer">The writer object.</param>
        private void WriteMigrationConfiguration(XmlWriter writer)
        {
            writer.WriteStartElement("Configuration");
            writer.WriteAttributeString("Date", DateTime.Now.ToShortDateString());
            writer.WriteAttributeString("ConnectionString", Environment.Configuration.GetConnectionString("DefaultConnection"));
            writer.WriteAttributeString("StartTime", _entries.FirstOrDefault()?.StartRunTime.ToShortTimeString());
            writer.WriteAttributeString("EndTime", _entries.LastOrDefault()?.EndRunTime.ToShortTimeString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the configuration of the Migration Tool report using <see cref="XmlWriter"/> asynchrously.
        /// </summary>
        /// <param name="writer">The writer object.</param>
        private async Task WriteMigrationConfigurationAsync(XmlWriter writer)
        {

            await writer.WriteStartElementAsync(string.Empty, "Configuration", string.Empty);
            await writer.WriteAttributeStringAsync(string.Empty, "Date", string.Empty, DateTime.Now.ToShortDateString());
            await writer.WriteAttributeStringAsync(string.Empty, "ConnectionString", string.Empty, Environment.Configuration.GetConnectionString("DefaultConnection"));
            await writer.WriteAttributeStringAsync(string.Empty, "StartTime", string.Empty, _entries.FirstOrDefault()?.StartRunTime.ToShortTimeString());
            await writer.WriteAttributeStringAsync(string.Empty, "EndTime", string.Empty, _entries.LastOrDefault()?.EndRunTime.ToShortTimeString());
            await writer.WriteEndElementAsync();
        }
        #endregion

        /// <summary>
        /// Invokes the  <see cref="ExecutionReportModified"/> delegate.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="entry">The report entry <see cref="ReportEntry"/>.</param>
        private void RaiseModified(string message, ReportEntry entry)
        {
            Modified?.Invoke(this, new[] {
                new ExecutionReportModifiedArgs{
                    Message = message,
                    ReportEntry = entry
                }
            });
        }
    }

}
