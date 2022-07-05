// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Medallion.Threading.FileSystem;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This the DistributedLock FileSystem to lock on a global directory available to all instances of the application
    /// </summary>
    public class FileSystemLockAndRunJob : ILockAndRunJob
    {
        private readonly string _directoryFilePath;
        private readonly RunSequentiallyOptions _options;

        /// <summary>
        /// This contains the name of the resource that this version is looking for
        /// </summary>
        public string ResourceName { get; }

        /// <summary>
        /// Ctor - gets the options and directory FilePath
        /// </summary>
        /// <param name="options"></param>
        /// <param name="directoryFilePath"></param>
        public FileSystemLockAndRunJob(RunSequentiallyOptions options, string directoryFilePath)
        {
            _options = options;
            _directoryFilePath = directoryFilePath;

            ResourceName = $"Looking for directory at {directoryFilePath}";
        }

        /// <summary>
        /// This will obtain a lock on the global resource and the run the registered <see cref="IStartupServiceToRunSequentially"/>
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public async Task LockAndRunActionAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var lockFileDirectory = new DirectoryInfo(_directoryFilePath);
            var distributedLock = new FileDistributedLock(lockFileDirectory, _options.GlobalLockName);
            await using (await distributedLock.AcquireAsync(
                TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                await scopedServices.RunJobAsync();
            }
        }

        /// <summary>
        /// This runs the given async action within a lock on a global resource 
        /// </summary>
        /// <param name="actionAsync"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async ValueTask LockAndRunActionAsync(Func<ValueTask> actionAsync, RunSequentiallyOptions options)
        {
            var lockFileDirectory = new DirectoryInfo(_directoryFilePath);
            var distributedLock = new FileDistributedLock(lockFileDirectory, _options.GlobalLockName);
            await using (await distributedLock.AcquireAsync(
                             TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                await actionAsync(); 
            }
        }

        /// <summary>
        /// This runs the given sync action within a lock on a global resource 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public void LockAndRunAction(Action action, RunSequentiallyOptions options)
        {
            var lockFileDirectory = new DirectoryInfo(_directoryFilePath);
            var distributedLock = new FileDistributedLock(lockFileDirectory, _options.GlobalLockName);
            using (distributedLock.Acquire(
                             TimeSpan.FromSeconds(_options.DefaultLockTimeoutInSeconds)))
            {
                action();
            }
        }
    }
}