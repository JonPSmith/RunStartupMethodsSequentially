// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Data.SqlClient;
using Npgsql;

namespace RunMethodsSequentially.LockAndRunCode;

/// <summary>
/// Useful extension methods to get the name of a database from the connection string
/// </summary>
public static class DatabaseHelpers
{
    /// <summary>
    /// Gets the database name from the SqlServer connection string
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static string GetDatabaseNameFromSqlServerConnectionString(this string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        return builder.InitialCatalog;
    }

    /// <summary>
    /// Gets the database name from the PostgreSql connection string
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static string GetDatabaseNameFromPostgreSqlConnectionString(this string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return builder.Database;
    }

}