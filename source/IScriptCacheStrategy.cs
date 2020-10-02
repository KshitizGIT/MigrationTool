using MigrationTool.Crawler;

namespace MigrationTool
{
    public interface IScriptCacheStrategy
    {
        void CacheScript(SqlFileDetail sqlFile);
        bool HasScriptChanged(SqlFileDetail sqlFile);
    }
}
