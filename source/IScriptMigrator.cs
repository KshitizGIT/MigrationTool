using System.Threading.Tasks;

namespace MigrationTool
{
    /// <summary>
    ///Defines methods use to migrate over a script.
    /// </summary>
    public interface IScriptMigrator
    {
        /// <summary>
        /// Migrates a script.
        /// </summary>
        /// <param name="script">The script to migrate.</param>
        void Migrate(string script);

        /// <summary>
        /// Migrates a script asynchronously.
        /// </summary>
        /// <param name="script">The script to migrate.</param>
        Task MigrateAsync(string script);
    }
}
