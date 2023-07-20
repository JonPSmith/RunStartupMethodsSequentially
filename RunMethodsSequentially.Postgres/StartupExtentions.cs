// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using RunMethodsSequentially.LockAndRunCode;

namespace RunMethodsSequentially
{
    public static class StartupExtensions
    {
        /// <summary>
        /// This will lock using a PostgreSQL database. If the PostgreSQL database hasn't been created yet
        /// it will pass onto the next lock type, e.g. <see cref="AddFileSystemLockAndRunMethods"/>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionString">The connection string to the PostgreSQL database</param>
        public static void AddPostgreSqlLockAndRunMethods(this RunSequentiallyOptions options,
            string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            options.LockVersionsInOrder.Add(new TryLockVersion(
                new PostgreSqlDoesDatabaseExist(connectionString),
                new PostgreSqlLockAndRunJob(options, connectionString)));
        }
    }
}