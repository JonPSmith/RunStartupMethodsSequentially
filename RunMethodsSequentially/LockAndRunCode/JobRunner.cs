// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    public static class JobRunner
    {
        /// <summary>
        /// This runs the registered services once a lock has been achieved
        /// </summary>
        /// <param name="scopedServices">NOTE: This is provided as a scopedServices</param>
        public static async Task RunJobAsync(this IServiceProvider scopedServices)
        {
            var servicesToRun = scopedServices.GetServices<IServiceToCallWhileInLock>().ToArray();
            if (!servicesToRun.Any())
                throw new RunSequentiallyException(
                    "You have not registered any services to run when the lock is active. " +
                    $"Use the {nameof(StartupExtensions.RegisterServiceToRunInJob)}<T> extension method to register the services you want run during a lock.");
            var duplicates = servicesToRun.GroupBy(x => x.GetType())
                .Where(x => x.Count() > 1).ToArray();
            if (duplicates.Any())
                throw new RunSequentiallyException(
                    $"Some of your services registered by {nameof(StartupExtensions.RegisterServiceToRunInJob)}<T> extension method are duplicates. They are: "+
                    string.Join(", ", duplicates.Select(x => x.Key.Name)));
            foreach (var serviceToRun in servicesToRun)
            {
                await serviceToRun.RunMethodWhileInLockAsync();
            }
        }
    }
}