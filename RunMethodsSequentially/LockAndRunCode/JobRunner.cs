﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            var logger = scopedServices.GetRequiredService<ILogger<GetLockAndThenRunServices>>();

            var servicesToRun = scopedServices.GetServices<IStartupServiceToRunSequentially>().ToArray();
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

            //This orders the startup services by their OrderNum and then execute them
            foreach (var serviceToRun in servicesToRun.OrderBy(service => service.OrderNum))
            {
                await serviceToRun.ApplyYourChangeAsync(scopedServices);
                logger.LogInformation("The startup service class [{0}] was successfully executed.", serviceToRun.GetType().Name);
            }
        }
    }
}