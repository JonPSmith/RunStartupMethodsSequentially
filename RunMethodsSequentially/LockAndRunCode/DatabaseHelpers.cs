// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Data.SqlClient;
using Npgsql;

namespace RunMethodsSequentially.LockAndRunCode
{
    public static class DatabaseHelpers
    {
        public static string GetDatabaseNameFromSqlServerConnectionString(this string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }

        public static string GetDatabaseNameFromPostgreSqlConnectionString(this string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return builder.Database;
        }

    }
}