using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MigrationTool.Database
{
    /// <summary>
    /// Creates a <see cref="SqlCommand"/> to execute sql scripts.
    /// </summary>
    public class SqlCommandExecuter
    {
        private readonly SqlConnection _connection;

        /// <summary>
        /// Creates a new instance of SqlCommandExecuter using the provided <see cref="SqlConnection"/>.
        /// </summary>
        /// <param name="connection">The sql connection.</param>
        public SqlCommandExecuter(SqlConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Executes the sql command.
        /// </summary>
        /// <param name="script">The script to run.</param>
        public void Execute(string script)
        {
            var transaction = _connection.BeginTransaction();
            using (var command = _connection.CreateCommand())
            {
                command.Transaction =transaction;
                try
                {
                    command.CommandText = script;
                    command.CommandTimeout = 1800;
                    command.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Executes the sql command asynchronously.
        /// </summary>
        /// <param name="script">The script to run.</param>
        public async Task ExecuteAsync(string script)
        {

            var transaction = _connection.BeginTransaction();
            using (var command = _connection.CreateCommand())
            {
                command.Transaction = transaction;
                try
                {
                    command.CommandText = script;
                    command.CommandTimeout = 1800;
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                transaction.Commit();
            }
        }
    }
}
