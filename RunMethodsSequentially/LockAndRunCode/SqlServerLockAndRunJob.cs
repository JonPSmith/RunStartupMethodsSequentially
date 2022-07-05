// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Medallion.Threading.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This the DistributedLock SQL Server to lock on a global SQL Server database available to all instances of the application
    /// </summary>
    public class SqlServerLockAndRunJob : ILockAndRunJob
    {
        private readonly string _connectionString;
        private readonly RunSequentiallyOptions _options;

        /// <summary>
        /// This contains the name of the resource that this version is looking for
        /// </summary>
        public string ResourceName { get; }

        /// <summary>
        /// Ctor - needs the options and the connection string
        /// </summary>
        /// <param name="options"></param>
        /// <param name="connectionString"></param>
        public SqlServerLockAndRunJob(RunSequentiallyOptions options, string connectionString)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            ResourceName = $"SQL Server database with name [{connectionString.GetDatabaseNameFromSqlServerConnectionString()}]";
        }

        /// <summary>
        /// This will obtain a global lock and then call the <see cref="JobRunner"/> to run all the
        /// registered <see cref="IStartupServiceToRunSequentially"/> services
        /// </summary>
        /// <param name="serviceProvider"></param>
        public async Task LockAndRunActionAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var distributedLock = new SqlDistributedLock(_options.GlobalLockName, _connectionString);
            await using (await distributedLock.AcquireAsync(
                TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                await scopedServices.RunJobAsync();
            }
        }

        /// <summary>
        /// This runs the given async action within a global lock
        /// </summary>
        /// <param name="actionAsync"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async ValueTask LockAndRunActionAsync(Func<ValueTask> actionAsync, RunSequentiallyOptions options)
        {
            var distributedLock = new SqlDistributedLock(_options.GlobalLockName, _connectionString);
            await using (await distributedLock.AcquireAsync(
                             TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                await actionAsync();
            }
        }

        /// <summary>
        /// This runs the given sync action within a global lock
        /// </summary>
        /// <param name="action"></param>
        /// <param name="options"></param>
        public void LockAndRunAction(Action action, RunSequentiallyOptions options)
        {
            var distributedLock = new SqlDistributedLock(_options.GlobalLockName, _connectionString);
            using (distributedLock.Acquire(
                             TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                action();
            }
        }
    }
}