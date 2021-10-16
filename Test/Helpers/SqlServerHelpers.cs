// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Data.SqlClient;
using TestSupport.EfHelpers;

namespace Test.Helpers
{
    public static class SqlServerHelpers
    {
        public static bool DoesDatabaseExist(this string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "";
            var newConnectionString = builder.ToString();

            return newConnectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1;
        }

        public static void DeleteDatabase(this string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "";
            var newConnectionString = builder.ToString();

            if (newConnectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1)
                newConnectionString.ExecuteNonQuery("DROP DATABASE [" + databaseName + "]");
            if (newConnectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1)
                //it failed
                throw new InvalidOperationException($"Failed to deleted {databaseName}. Did you have SSMS open or something?");
        }

        public static void CreateDatabase(this string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "";
            var newConnectionString = builder.ToString();

            if (newConnectionString.ExecuteRowCount("sys.databases", $"WHERE [Name] = '{databaseName}'") == 1)
                throw new InvalidOperationException($"There is a data of that name already.");

            newConnectionString.ExecuteNonQuery("CREATE DATABASE [" + databaseName + "]");
        }
    }
}