// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Test.EfCore;
using TestSupport.Helpers;

namespace Test.Helpers
{
    public static class PostGreExtensions
    {
        public static DbContextOptions<TContext> CreatePostGreUniqueDatabaseOptions<TContext>(this object testClass, 
            string optionalMethodName = null, char separator = '_')
            where TContext : DbContext
        {
            var connectionString = testClass.GetUniquePostGreSqlDatabaseConnectionString(optionalMethodName, separator);

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return optionsBuilder.Options;
        }

        public static string GetUniquePostGreSqlDatabaseConnectionString(this object testClass,
            string optionalMethodName = null, char separator = '_')
        {
            var baseConnection = AppSettings.GetConfiguration().GetConnectionString("PostGreConnection");
            var builder = new NpgsqlConnectionStringBuilder(baseConnection);
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
    }
}