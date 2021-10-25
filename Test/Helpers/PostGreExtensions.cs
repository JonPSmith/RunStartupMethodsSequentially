// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Test.EfCore;
using TestSupport.EfHelpers;
using TestSupport.Helpers;

namespace Test.Helpers
{
    public static class PostgreExtensions
    {
        /// <summary>
        /// Returns the base connection for the PostgreSql Database
        /// NOTE: This connection is normally altered to create a  
        /// </summary>
        /// <returns></returns>
        public static string GetPostgreSqlDatabaseConnectionStringFromAppSettings()
        {
            return AppSettings.GetConfiguration().GetConnectionString("PostgreSqlConnection");
        }

        public static DbContextOptions<TContext> CreatePostgreUniqueDatabaseOptions<TContext>(this object testClass, 
            string optionalMethodName = null, char separator = '_')
            where TContext : DbContext
        {
            var connectionString = testClass.GetUniquePostgreSqlDatabaseConnectionString(optionalMethodName, separator);

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return optionsBuilder.Options;
        }

        public static DbContextOptions<T> CreatePostgreUniqueDatabaseOptionsWithLogging<T>(this object callingClass,
            Action<string> logAction)
            where T : DbContext
        {
            if (logAction == null) throw new ArgumentNullException(nameof(logAction));

            var optionsBuilder = new DbContextOptionsBuilder<T>();
            var connectionString = callingClass.GetUniquePostgreSqlDatabaseConnectionString();
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.LogTo(logAction, LogLevel.Information, Microsoft.EntityFrameworkCore.Diagnostics.DbContextLoggerOptions.None);

            return optionsBuilder.Options;
        }

        public static string GetUniquePostgreSqlDatabaseConnectionString(this object testClass,
            string optionalMethodName = null, char separator = '_')
        {
            var builder = new NpgsqlConnectionStringBuilder(GetPostgreSqlDatabaseConnectionStringFromAppSettings());
            if (!builder.Database.EndsWith(AppSettings.RequiredEndingToUnitTestDatabaseName))
                throw new InvalidOperationException($"The database name in your connection string must end with '{AppSettings.RequiredEndingToUnitTestDatabaseName}', but is '{builder.Database}'." +
                                                    " This is a safety measure to help stop DeleteAllUnitTestDatabases from deleting production databases.");

            var extraDatabaseName = $"{separator}{testClass.GetType().Name}";
            if (!string.IsNullOrEmpty(optionalMethodName)) extraDatabaseName += $"{separator}{optionalMethodName}";
            builder.Database += extraDatabaseName;

            return builder.ToString();
        }

        public static void ClearCreatedAndEmpty(this TestDbContext context)
        {
            context.Database.EnsureCreated();

            context.RemoveRange(context.CommonNameDateTimes);
            context.RemoveRange(context.NameDateTimes);
            context.SaveChanges();

            context.ChangeTracker.Clear();
        }


        /// <summary>
        /// This will delete all the databases that start with the database name in the default connection string
        /// WARNING: This will delete multiple databases - make sure your DefaultConnection database name is unique!!!
        /// </summary>
        /// <returns>Number of databases deleted</returns>
        public static int DeleteAllUnitTestDatabases()
        {
            var baseConnection = GetPostgreSqlDatabaseConnectionStringFromAppSettings();
            var databaseNamesToDelete = baseConnection.GetAllPostgreUnitTestDatabases();

            var builder = new NpgsqlConnectionStringBuilder(baseConnection);
            builder.Database = "postgres";
            foreach (var databaseName in databaseNamesToDelete)
            {
                //The following commands were taken from EF Core
                //also see https://www.postgresqltutorial.com/postgresql-drop-database/ for another form
                using (NpgsqlConnection conn = new NpgsqlConnection(builder.ToString()))
                {
                    void ExecuteScalar(string cmdText)
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
                        {
                            var result = cmd.ExecuteScalar();
                        }
                    }

                    conn.Open();
                    ExecuteScalar($"REVOKE CONNECT ON DATABASE \"{databaseName}\" FROM PUBLIC");                  
                    ExecuteScalar("SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity " +
                        $"WHERE datname = '{databaseName}'");
                    ExecuteScalar($"DROP DATABASE \"{databaseName}\"");
                }
            }
            return databaseNamesToDelete.Count;
        }

        public static List<string> GetAllPostgreUnitTestDatabases(this string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var orgDbStartsWith = builder.Database;
            builder.Database = "postgres";
            var newConnectionString = builder.ToString();

            var result = new List<string>();
            using (NpgsqlConnection conn = new NpgsqlConnection(newConnectionString))
            {
                conn.Open();
                string cmdText = $"SELECT datName FROM pg_database WHERE datname LIKE '{orgDbStartsWith}%'";
                using (NpgsqlCommand cmd = new NpgsqlCommand(cmdText, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetString(0));
                        }
                    }
                }
            }

            return result;
        }
    }
}