// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Data.SqlClient;

namespace RunMethodsSequentially.LockAndRunCode
{
    public static class SqlServerHelpers
    {
        public static string GetDatabaseNameFromConnectionString(this string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }

    }
}