// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially.LockAndRunCode;

namespace RunMethodsSequentially
{
    public static class StartupExtensions
    {
        /// <summary>
        /// This registers the RunMethodsSequentially feature into your DI services, 
        /// and you then register how you want to lock the your global resource(s) via the options.
        /// NOTE: By default the RunMethodsSequentially code will be registered as a IHostedService,
        /// which is correct for usage in a ASP.NET Core. If you aren't usinh ASP.NET Core, then see the 
        /// docs on how to register RunMethodsSequentially as a normal service
        /// </summary>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
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

        /// <summary>
        /// This will lock using a SQL Server database. If the SQL Server database hasn't been created yet
        /// it will pass onto the next lock type, e.g. <see cref="AddFileSystemLockAndRunMethods"/>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionString">The connection string to the SQL Server database</param>
        public static void AddSqlServerLockAndRunMethods(this RunSequentiallyOptions options,
            string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

            options.LockVersionsInOrder.Add(new TryLockVersion(
                new SqlServerDoesDatabaseExist(connectionString),
                new SqlServerLockAndRunJob(options, connectionString)));
        }

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

        /// <summary>
        /// This will lock on a filesytem directory in your running application, e.g. 
        /// in ASP.NET Core the wwwroot directory. If it can't find the directory it will pass onto
        /// the next lock type. If there isn't a next lock type it will fail
        /// </summary>
        /// <param name="options"></param>
        /// <param name="directoryFilePath">The filepath to a global directory accessable by all the instances of your app</param>
        public static void AddFileSystemLockAndRunMethods(this RunSequentiallyOptions options,
            string directoryFilePath)
        {
            if (directoryFilePath == null) throw new ArgumentNullException(nameof(directoryFilePath));

            options.LockVersionsInOrder.Add(new TryLockVersion(
                new FileSystemDoesDirectoryExist(directoryFilePath),
                new FileSystemLockAndRunJob(options, directoryFilePath)));
        }

        /// <summary>
        /// This just runs the your startup services without locking anything.
        /// This is useful if you are only running one instance of your application
        /// NOTE: Locking a database is fast (1 ms local SQL Server), but no lock only takes 50 us
        /// </summary>
        /// <param name="options"></param>
        public static void AddRunMethodsWithoutLock(this RunSequentiallyOptions options)
        {
            options.LockVersionsInOrder.Add(new TryLockVersion(
                new NoLockPreLockTest(),
                new NoLockAndRunJob()));
        }

        /// <summary>
        /// This method allows you to register your startup services, i.e. classes that inherit the
        /// interface called <see cref="IStartupServiceToRunSequentially"/>.
        /// NOTE that the order in which your startup services are registered defines the order they are executed,
        /// BUT a <see cref="IStartupServiceToRunSequentially.OrderNum"/> value of not zero overides the order of execution
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        public static RunSequentiallyOptions RegisterServiceToRunInJob<TService>(this RunSequentiallyOptions options)
            where TService : class, IStartupServiceToRunSequentially
        {
            options.Services.AddTransient<IStartupServiceToRunSequentially, TService>();
            return options;
        }
    }
}