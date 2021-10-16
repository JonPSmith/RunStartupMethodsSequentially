// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially.LockAndRunCode;

namespace RunMethodsSequentially
{
    public class RunSequentiallyOptions
    {

        public string LockName { get; set; } = nameof(RunMethodsSequentially);

        public int DefaultLockTimeoutInSeconds { get; set; } = 100;

        public bool RegisterAsHostedService { get; set; } = true;

        public ICollection<TryLockVersion> LockVersionsInOrder { get; } = new List<TryLockVersion>();

        public RunSequentiallyOptions(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }
    }
}