// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Npgsql;

namespace RunMethodsSequentially.LockAndRunCode
{
    public class PostgreSqlDoesDatabaseExist : IPreLockTest
    {
        private readonly string _connectionString;

        public PostgreSqlDoesDatabaseExist(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async ValueTask<bool> CheckLockResourceExistsAsync()
        {
            //Thanks to phil_rawlings for his stack overflow answer https://stackoverflow.com/a/20032567/1434764
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            var databaseToLookFor = builder.Database;
            builder.Database = "postgres";
            var newConnectionString = builder.ToString();

            using (NpgsqlConnection conn = new NpgsqlConnection(newConnectionString))
            {
                conn.Open();
                string cmdText = $"SELECT 1 FROM pg_database WHERE datname='{databaseToLookFor}'";
                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
                {
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null;
                }
            }
        }
    }
}