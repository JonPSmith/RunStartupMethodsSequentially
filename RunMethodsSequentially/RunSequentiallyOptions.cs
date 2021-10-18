// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Medallion.Threading;
using RunMethodsSequentially.LockAndRunCode;

namespace RunMethodsSequentially
{
    public class RunSequentiallyOptions
    {
        /// <summary>
        /// This contains the name of the global lock, default is <see cref="RunMethodsSequentially"/>.
        /// Useful to know in case you want to use other <see cref="IDistributedLock"/> services
        /// </summary>
        public string GlobalLockName { get; set; } = nameof(RunMethodsSequentially);

        /// <summary>
        /// This defines how long it will try to obtain a lock, defaults to 100 seconds
        /// When you have `NNN` multiple instances the total time taken for the ALL of your
        /// startup services to run must be less that `DefaultLockTimeoutInSeconds / NNN`.
        /// </summary>
        public int DefaultLockTimeoutInSeconds { get; set; } = 100;

        /// <summary>
        /// This defines how the <see cref="GetLockAndThenRunServices"/> is registered in the dependency injection provider
        /// Default is true, with registers the code as a <see cref="IHostedService"/>
        /// If false, it is registered as transient service found by the <see cref="IGetLockAndThenRunServices"/>
        /// </summary>
        public bool RegisterAsHostedService { get; set; } = true;

        /// <summary>
        /// This contains a series of Lock Versions. It will start with the first one and only steps the next
        /// Lock Version(s) if the resource isn't found.
        /// </summary>
        public ICollection<TryLockVersion> LockVersionsInOrder { get; } = new List<TryLockVersion>();

        public RunSequentiallyOptions(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }
    }
}