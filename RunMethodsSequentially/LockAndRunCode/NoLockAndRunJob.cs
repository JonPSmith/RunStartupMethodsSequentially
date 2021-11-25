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
    public class NoLockAndRunJob : ILockAndRunJob
    {
        public string ResourceName { get; } = "No locking applied";

        public async Task LockAndRunMethodsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;      
            await scopedServices.RunJobAsync();
        }
    }
}