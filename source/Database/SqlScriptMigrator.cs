using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MigrationTool.Database
{
    /// <summary>
    /// Migrates script via sql connection.
    /// </summary>
    public class SqlScriptMigrator : IScriptMigrator
    {
        private SqlConnection _connection;
        private SqlCommandExecuter _sqlCommandExecuter;

        /// <summary>
        /// Creates a new SqlScriptMigration using the given connection string.
        /// </summary>
        /// <param name="connectionString">The sql connection string.</param>
        public SqlScriptMigrator(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// The sql connection used. Opens the connection if closed.
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


        /// <summary>
        /// Migrates a script using the configured <see cref="SqlConnection"/>.
        /// </summary>
        /// <param name="script">The script to migrate.</param>
        public void Migrate(string script)
        {
            _sqlCommandExecuter = new SqlCommandExecuter(SqlConnection);
            try
            {
                _sqlCommandExecuter.Execute(script);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Migrates a script using the configured <see cref="SqlConnection"/> asynchronously.
        /// </summary>
        /// <param name="script">The script to migrate.</param>
        public async Task MigrateAsync(string script)
        {
            _sqlCommandExecuter = new SqlCommandExecuter(SqlConnection);
            try
            {
                await _sqlCommandExecuter.ExecuteAsync(script);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
