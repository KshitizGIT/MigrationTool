using log4net;
using Microsoft.Extensions.Configuration;
using MigrationTool.Analyzer;
using MigrationTool.Crawler;
using MigrationTool.Database;
using MigrationTool.Reporting;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MigrationTool
{
    /// <summary>
    /// Class BaseMigrator. Gets a list of sql files, analyzes it and executes it.
    /// Maintains history of the executed scripts.
    /// </summary>
    public abstract class BaseMigrator
    {
        private string _reportFileName;
        private string _reportFilePath;
        private IScriptCacheStrategy _scriptCacheStrategy;
        private readonly IScriptAnalyzer _scriptAnalyzer;
        private ISqlFilesLister _filesLister;
        private readonly IScriptMigrator _scriptMigrator;
        protected ILog Log = LogManager.GetLogger(Assembly.GetExecutingAssembly(), "MigrationTool.Migrator");


        protected IScriptMigrator ScriptMigrator
        {
            get { return _scriptMigrator; }
        }
        protected ISqlFilesLister SqlFilesLister
        {
            get { return _filesLister; }
            set { _filesLister = value; }
        }

        protected ExecutionReport ExecutionReport { get; private set; } = new ExecutionReport();

        public BaseMigrator(ISqlFilesLister lister, IScriptMigrator migrator) : this(lister, new ScriptAnalyzer(), migrator)
        {

        }
        public BaseMigrator(ISqlFilesLister lister, IScriptAnalyzer analyzer, IScriptMigrator migrator)
        { 
            _filesLister = lister;
            _scriptAnalyzer = analyzer;
            _scriptMigrator = migrator;
            _scriptCacheStrategy = new SqlMigrationHistory(Environment.Configuration.GetConnectionString("DefaultConnection"));
        }


        public string ReportPath
        {
            get { return _reportFilePath; }
            set { _reportFilePath = value.EndsWith(Path.DirectorySeparatorChar) ? value : value + Path.DirectorySeparatorChar; }
        }


        public string ReportFileName
        {
            get
            {
                return _reportFileName ??
                  (_reportFileName = $"{ReportPath}MigrationReport_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xml");
            }
        }

        public string ProcessingDirectory
        {
            get { return _filesLister.FilesLocation; }
        }

        public void Run()
        {
            Log.Info($"Running Migration Tool ..... ");
            foreach (var file in _filesLister.GetSqlFilesWithContents())
            {
                var entry = new ReportEntry
                {
                    File = file.FullPath,
                    StartRunTime = DateTime.Now,
                    Status = "Success"
                };
                try
                {
                    //check if file has been modified before executing it.
                    if (!_scriptCacheStrategy.HasScriptChanged(file))
                    {
                        entry.Status = "Skipped";
                        continue;
                    }
                    Execute(file);

                    _scriptCacheStrategy.CacheScript(file);
                }
                catch (Exception ex)
                {
                    entry.Exception = ex.Message;
                    entry.ReportEntryType = ReportEntryType.Error;
                    entry.Status = "Fail";
                }
                finally
                {
                    entry.EndRunTime = DateTime.Now;
                    ExecutionReport.AddEntry(entry);
                }
            }
            try
            {
                CreateLogViewerFile();

                Log.Info("Writing reports.....");
                ExecutionReport.WriteReport(ReportFileName);
                Log.Info("Finished running migration tool.");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public virtual void Execute(SqlFileDetail file)
        {
            var scripts = _scriptAnalyzer.GetScriptsToRun(file);
            try
            {
                foreach (var script in scripts)
                {
                    _scriptMigrator.Migrate(script);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RunAsync()
        {
            Log.Info($"Running Migration Tool ..... ");
            foreach (var file in _filesLister.GetSqlFilesWithContents())
            {
                var entry = new ReportEntry
                {
                    File = file.FullPath,
                    StartRunTime = DateTime.Now,
                    Status = "Success"
                };
                try
                {
                    //check if file has been modified before executing it.
                    if (!_scriptCacheStrategy.HasScriptChanged(file))
                    {
                        entry.Status = "Skipped";
                        continue;
                    }
                    await ExecuteAsync(file);

                    // maintain history if executed successfully.
                    _scriptCacheStrategy.CacheScript(file);

                }
                catch (Exception ex)
                {
                    entry.Exception = ex.Message;
                    entry.ReportEntryType = ReportEntryType.Error;
                    entry.Status = "Fail";
                }
                finally
                {
                    entry.EndRunTime = DateTime.Now;
                    ExecutionReport.AddEntry(entry);
                }
            }
            try
            {
                Log.Info("Writing reports.....");
                var createLogViewerWork = CreateLogViewerFileAsync();
                await ExecutionReport.WriteReportAsync(ReportFileName);
                await createLogViewerWork;
                Log.Info("Finished running migration tool.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public virtual async Task ExecuteAsync(SqlFileDetail file)
        {
            try
            {
                //analyze scripts here. Statements like GO discarded
                var scripts = _scriptAnalyzer.GetScriptsToRun(file);

                foreach (var script in scripts)
                    await _scriptMigrator.MigrateAsync(script);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void CreateLogViewerFile()
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (!Directory.Exists(ReportPath))
                Directory.CreateDirectory(ReportPath);

            var stream = assembly.GetManifestResourceStream("Reporting.MigrationReportViewer.format.xsl");
            using (var streamReader = new StreamReader(stream))
            {
                var streamContent = streamReader.ReadToEnd();
                File.WriteAllText($"{ReportPath}MigrationReportViewer.xsl", streamContent);
            }
        }

        private async Task CreateLogViewerFileAsync()
        {
            if (!Directory.Exists(ReportPath))
                Directory.CreateDirectory(ReportPath);

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("MigrationTool.Reporting.MigrationReportViewer.format.xsl");
            using (var streamReader = new StreamReader(stream))
            {
                var streamContent = await streamReader.ReadToEndAsync();
                await File.WriteAllTextAsync($"{ReportPath}MigrationReportViewer.xsl", streamContent);
            }
        }
    }
}
