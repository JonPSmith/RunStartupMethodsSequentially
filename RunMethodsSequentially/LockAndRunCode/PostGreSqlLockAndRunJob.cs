// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Medallion.Threading.Postgres;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This the DistributedLock Postgres to lock on a global Postgres database available to all instances of the application
    /// </summary>
    public class PostgreSqlLockAndRunJob : ILockAndRunJob
    {
        private readonly string _connectionString;
        private readonly RunSequentiallyOptions _options;

        public string ResourceName { get; }

        public PostgreSqlLockAndRunJob(RunSequentiallyOptions options, string connectionString)
        {
            _options = options;
            _connectionString = connectionString;

            ResourceName = $"PostgreSQL database with name [{connectionString.GetDatabaseNameFromPostgreSqlConnectionString()}]";
        }

        public async Task LockAndRunMethodsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var distributedLock = new PostgresDistributedLock(new PostgresAdvisoryLockKey(_options.GlobalLockName, allowHashing: true), _connectionString);
            await using (await distributedLock.AcquireAsync(
                TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                await scopedServices.RunJobAsync();
            }
        }
    }
}