using System.IO;
using System.Security.Cryptography;

namespace MigrationTool
{
    public static class HashSumAnalyzer
    {
        /// <summary>
        /// Get hash sum of a file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static byte[] GetFileHash(string fileName)
        {
            SHA1 sha1 = SHA1.Create();
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return sha1.ComputeHash(stream);
            }
        }
    }
}
