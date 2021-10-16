// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace RunStartupMethodsSequentially
{
    public class RunSequentiallyOptions
    {

        public string LockDatabaseName { get; set; } = nameof(LockDatabaseAndRunJob);

        public int DefaultLockTimeoutInSeconds { get; set; } = 100;

        public RunSequentiallyOptions(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public ICollection<TryLockVersion> LockVersionsInOrder { get; } = new List<TryLockVersion>();
    }
}