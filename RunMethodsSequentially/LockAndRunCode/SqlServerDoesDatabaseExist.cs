// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace RunMethodsSequentially.LockAndRunCode
{
    public class SqlServerDoesDatabaseExist : IPreLockTest
    {
        private readonly string _connectionString;

        public SqlServerDoesDatabaseExist(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async ValueTask<bool> CheckLockResourceExistsAsync()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "";
            var newConnectionString = builder.ToString();

            using (var myConn = new SqlConnection(newConnectionString))
            {
                var command = $"SELECT COUNT(*) FROM sys.databases WHERE [Name] = '{databaseName}'";
                var myCommand = new SqlCommand(command, myConn);
                myConn.Open();
                return ((int)await myCommand.ExecuteScalarAsync()) == 1;
            }
        }
    }
}