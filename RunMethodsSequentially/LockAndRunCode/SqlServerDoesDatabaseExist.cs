// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This checks that a SQL Server database defined in its connection string exists
    /// </summary>
    public class SqlServerDoesDatabaseExist : IPreLockTest
    {
        private readonly string _connectionString;

        /// <summary>
        /// Ctor - needs SqlServer connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlServerDoesDatabaseExist(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// This returns true if the SqlServer Database exists
        /// </summary>
        /// <returns></returns>
        public ValueTask<bool> CheckLockResourceExistsAsync()
        {
            return CheckLockResourceExists(true);
        }

        /// <summary>
        /// This returns true if the SqlServer Database exists
        /// </summary>
        /// <returns></returns>
        public bool CheckLockResourceExists()
        {
            return CheckLockResourceExists(false).CheckSyncValueTaskWorkedAndReturnResult();
        }

        //--------------------------------------------------------------
        //private methods

        private async ValueTask<bool> CheckLockResourceExists(bool useAsync)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "";
            var newConnectionString = builder.ToString();

            using var myConn = new SqlConnection(newConnectionString);
            var command = $"SELECT COUNT(*) FROM sys.databases WHERE [Name] = '{databaseName}'";
            var myCommand = new SqlCommand(command, myConn);
            myConn.Open();
            return useAsync
                ? ((int)await myCommand.ExecuteScalarAsync()) == 1
                : ((int)myCommand.ExecuteScalar()) == 1;
        }
    }
}