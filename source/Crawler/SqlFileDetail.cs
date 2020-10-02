namespace MigrationTool.Crawler
{
    /// <summary>
    /// POCO class representing sql file with details.
    /// </summary>
    public class SqlFileDetail
    {
       /// <summary>
       /// Name of the sql file.
       /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The fully qualified path of the sql file.
        /// </summary>
        public string FullPath{ get; set; }
        /// <summary>
        /// The cryptographic hash sum of the sql file.
        /// </summary>
        public byte[] HashSum { get; set; }
        /// <summary>
        /// The content of the sql file.
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// The type of sql file. May be proc, sql.
        /// </summary>
        public string Type { get; set; }

    }
}
