// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Npgsql;

namespace RunMethodsSequentially.LockAndRunCode;

/// <summary>
/// This checks that a PostgreSQL database defined in its connection string exists
/// </summary>
public class PostgreSqlDoesDatabaseExist : IPreLockTest
{
    private readonly string _connectionString;

    /// <summary>
    /// Ctor - needs the connection string
    /// </summary>
    /// <param name="connectionString"></param>
    public PostgreSqlDoesDatabaseExist(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Returns true if the PostgreSql database exists
    /// </summary>
    /// <returns></returns>
    public ValueTask<bool> CheckLockResourceExistsAsync()
    {
        return CheckLockResourceExists(true);
    }

    /// <summary>
    /// Returns true if the PostgreSql database exists
    /// </summary>
    /// <returns></returns>
    public bool CheckLockResourceExists()
    {
        return CheckLockResourceExists(false).CheckSyncValueTaskWorkedAndReturnResult();
    }

    //-------------------------------------------------------------
    //private method

    private async ValueTask<bool> CheckLockResourceExists(bool useAsync)
    {
        //Thanks to phil_rawlings for his stack overflow answer https://stackoverflow.com/a/20032567/1434764
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseToLookFor = builder.Database;
        builder.Database = "postgres";
        var newConnectionString = builder.ToString();

        using NpgsqlConnection conn = new NpgsqlConnection(newConnectionString);
        conn.Open();
        string cmdText = $"SELECT 1 FROM pg_database WHERE datname='{databaseToLookFor}'";
        using NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn);
        var result = useAsync
            ? await cmd.ExecuteScalarAsync()
            : cmd.ExecuteNonQuery();
        return result != null;
    }
}