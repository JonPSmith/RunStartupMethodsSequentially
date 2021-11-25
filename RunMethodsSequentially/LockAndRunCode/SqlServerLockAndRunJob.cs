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

        public string ResourceName { get; }

        public SqlServerLockAndRunJob(RunSequentiallyOptions options, string connectionString)
        {
            _options = options;
            _connectionString = connectionString;

            ResourceName = $"SQL Server database with name [{connectionString.GetDatabaseNameFromSqlServerConnectionString()}]";
        }

        public async Task LockAndRunMethodsAsync(IServiceProvider serviceProvider)
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
    }
}