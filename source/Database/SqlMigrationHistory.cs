using MigrationTool.Crawler;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace MigrationTool.Database
{
    /// <summary>
    /// Maintains history for executed sql scripts. 
    /// By default, history is maintained in a table named __GBD_MigrationHistory.
    /// </summary>
    public class SqlMigrationHistory : IScriptCacheStrategy
    {
        private readonly SqlConnection _connection;
        private const string HISTORYTABLENAME = "__GBD_MigrationHistory";

        /// <summary>
        /// Creates a new instance of SqlMigrationHistory.
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlMigrationHistory(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
            EnsureMigrationTableCreated();
        }

        /// <summary>
        /// The sql connection to save executed scripts.
        /// </summary>
        public SqlConnection SqlConnection
        {
            get
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }
                return _connection;
            }
        }

        #region private
        /// <summary>
        /// Ensures that the history table exists.
        /// </summary>
        private void EnsureMigrationTableCreated()
        {
            var cmdText = $@"IF OBJECT_ID(N'[dbo].[{HISTORYTABLENAME}]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[{HISTORYTABLENAME}]
                    (
                        FileName [nvarchar](4000) NOT NULL,
                        HashSum [varbinary](max) NOT NULL
                    );
                END
                ELSE
                BEGIN
                    PRINT 'Table {HISTORYTABLENAME} EXISTS'
                END";

            using (var cmd = new SqlCommand(cmdText, SqlConnection))
            {
                cmd.ExecuteNonQuery();
            }

            EnsureIndexOnMigrationTable();
        }
        /// <summary>
        /// Adds index on the history table. 
        /// </summary>
        private void EnsureIndexOnMigrationTable()
        {
            var cmdText = $@"IF NOT EXISTS(SELECT 1 from sys.indexes where name ='fileNameHistoryIndex' AND object_id = OBJECT_ID(N'[dbo].[{HISTORYTABLENAME}]'))
                    BEGIN
                        CREATE INDEX fileNameHistoryIndex on {HISTORYTABLENAME} (FileName)
                    END
                    ELSE
                    BEGIN
                        PRINT 'Index already exists on {HISTORYTABLENAME}'
                    END ";

            using (var cmd = new SqlCommand(cmdText, SqlConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Helper function to retriev files hash sum information.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private byte[] GetHashSum(SqlFileDetail file)
        {
            var cmdText = $"SELECT HashSum from {HISTORYTABLENAME} where FileName = @file";
            try
            {
                using (var cmd = new SqlCommand(cmdText, SqlConnection))
                {
                    cmd.Parameters.Add(new SqlParameter("@file", file.FullPath));
                    return (byte[])cmd.ExecuteScalar();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Saves the information of executed <see cref="SqlFileDetail"/>.
        /// </summary>
        /// <param name="file"></param>
        private void MaintainMigrationHistory(SqlFileDetail file)
        {
            var cmdText = $@"IF EXISTS(SELECT 1 FROM {HISTORYTABLENAME} where FileName = @file)
                           BEGIN
                            UPDATE {HISTORYTABLENAME}  SET HashSum = @hashsum
                                where FileName = @file
                           END
                           ELSE
                            BEGIN
                            INSERT INTO {HISTORYTABLENAME}(FileName, HashSum) VALUES(@file, @hashsum)
                            END";
            try
            {
                using (var cmd = new SqlCommand(cmdText, SqlConnection))
                {
                    cmd.Parameters.Add(new SqlParameter("@file", file.FullPath));
                    cmd.Parameters.Add(new SqlParameter("@hashsum", file.HashSum));
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        #endregion

        public void CacheScript(SqlFileDetail sqlFile)
        {
            try
            {
                MaintainMigrationHistory(sqlFile);
            }
            catch (Exception)
            {

                throw;
            }

        }

        public bool HasScriptChanged(SqlFileDetail sqlFile)
        {
            try
            {
                var byteArray = GetHashSum(sqlFile);
                return byteArray == null || !byteArray.SequenceEqual(sqlFile.HashSum);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
