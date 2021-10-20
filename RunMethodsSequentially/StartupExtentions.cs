// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially.LockAndRunCode;

namespace RunMethodsSequentially
{
    public static class StartupExtensions
    {
        public static RunSequentiallyOptions RegisterRunMethodsSequentially(this IServiceCollection services,
            Action<RunSequentiallyOptions> optionsAction = null)
        {
            var options = new RunSequentiallyOptions(services);
            optionsAction?.Invoke(options);

            services.AddSingleton(options);
            if (options.RegisterAsHostedService)
                services.AddHostedService<GetLockAndThenRunHostedService>();
            else
                services.AddTransient<IGetLockAndThenRunServices, GetLockAndThenRunServices>();

            return options;
        }

        public static void AddSqlServerLockAndRunMethods(this RunSequentiallyOptions options,
            string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            options.LockVersionsInOrder.Add(new TryLockVersion(
                new SqlServerDoesDatabaseExist(connectionString),
                new SqlServerLockAndRunJob(options, connectionString)));
        }

        public static void AddPostGreSqlLockAndRunMethods(this RunSequentiallyOptions options,
            string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            options.LockVersionsInOrder.Add(new TryLockVersion(
                new PostGreSqlDoesDatabaseExist(connectionString),
                new PostGreSqlLockAndRunJob(options, connectionString)));
        }

        public static void AddFileSystemLockAndRunMethods(this RunSequentiallyOptions options,
            string directoryFilePath)
        {
            if (directoryFilePath == null) throw new ArgumentNullException(nameof(directoryFilePath));

            options.LockVersionsInOrder.Add(new TryLockVersion(
                new FileSystemDoesDirectoryExist(directoryFilePath), 
                new FileSystemLockAndRunJob(options, directoryFilePath)));
        }

        public static RunSequentiallyOptions RegisterServiceToRunInJob<TService>(this RunSequentiallyOptions options)
            where TService : class, IServiceToCallWhileInLock
        {
            options.Services.AddTransient<IServiceToCallWhileInLock, TService>();
            return options;
        }
    }
}