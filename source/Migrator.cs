using Microsoft.Extensions.Configuration;
using MigrationTool.Crawler;
using MigrationTool.Database;

namespace MigrationTool
{
    public class Migrator : BaseMigrator
    {

        public Migrator(string sqlFilesLocation, char mode) : base(null , new SqlScriptMigrator(Environment.Configuration.GetConnectionString("DefaultConnection")))
        {
            ScriptMigrator.SqlConnection.InfoMessage += SqlConnection_InfoMessage;
            ExecutionReport.Modified += ExecutionReport_Modified;
            if (mode == 'f')
                SqlFilesLister = new SingleFileLister(sqlFilesLocation);
            else if (mode =='d')
                SqlFilesLister = new DirectoryFilesLister(sqlFilesLocation);
            else if (mode == 'r')
                SqlFilesLister = new DirectoryRecursiveLister(sqlFilesLocation);
        }


        public new SqlScriptMigrator ScriptMigrator
        {
            get
            {
                return base.ScriptMigrator as SqlScriptMigrator;
            }
        }

        private void SqlConnection_InfoMessage(object sender, System.Data.SqlClient.SqlInfoMessageEventArgs e)
        {
            Log.Info(e.Message);
        }

        private void ExecutionReport_Modified(object sender, Reporting.ExecutionReportModifiedArgs[] args)
        {
            if (args[0].ReportEntry.ReportEntryType == Reporting.ReportEntryType.Error)
                Log.Error($"{ args[0].Message} : Status = {args[0].ReportEntry.Status} : Exception = {args[0].ReportEntry.Exception}");
            else
                Log.Info($"{args[0].Message} : Status =  {args[0].ReportEntry.Status}");
        }
    }
}
