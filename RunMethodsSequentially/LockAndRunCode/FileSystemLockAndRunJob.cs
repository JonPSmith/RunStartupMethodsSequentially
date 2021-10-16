// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Medallion.Threading.FileSystem;
using Medallion.Threading.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    public class FileSystemLockAndRunJob : ILockAndRunJob
    {
        private readonly string _directoryFilePath;
        private readonly RunSequentiallyOptions _options;

        public string ResourceName { get; }

        public FileSystemLockAndRunJob(RunSequentiallyOptions options, string directoryFilePath)
        {
            _options = options;
            _directoryFilePath = directoryFilePath;

            ResourceName = $"Looking for directory at {directoryFilePath}";
        }

        public async Task LockAndRunMethodsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var lockFileDirectory = new DirectoryInfo(_directoryFilePath);
            var distributedLock = new FileDistributedLock(lockFileDirectory, _options.LockName);
            await using (await distributedLock.AcquireAsync(
                TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                await scopedServices.RunJobAsync();
            }

        }
    }
}