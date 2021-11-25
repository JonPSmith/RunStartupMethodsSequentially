// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This applies the registered lock types and tries each in order until it has a lock.
    /// If no lock can be found then it throws an exception.
    /// If it gets a lock, then it will execute the startup services that you have registered
    /// 
    /// </summary>
    public class GetLockAndThenRunServices : IGetLockAndThenRunServices
    {
        private readonly IServiceProvider _serviceProvider;

        public GetLockAndThenRunServices(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// This uses the PreLockCheck to check the the resource to be locked exists.
        /// If the resource does exist it calls the code to lock and run the methods.
        /// If no resource is found, then it throws an exception
        /// </summary>
        /// <returns></returns>
        public async Task<bool> LockAndLoadAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var options = scopedServices.GetRequiredService<RunSequentiallyOptions>();
            if (!options.LockVersionsInOrder.Any())
                throw new RunSequentiallyException(
                    $"You must register at least one lock service when registering {nameof(StartupExtensions.RegisterRunMethodsSequentially)}, " +
                    $"for instance services.{nameof(StartupExtensions.RegisterRunMethodsSequentially)}(options => options.{nameof(StartupExtensions.AddSqlServerLockAndRunMethods)}(connectionString))...");
            foreach (var lockVersion in options.LockVersionsInOrder)
            {
                if (await lockVersion.PreLockCheck.CheckLockResourceExistsAsync())
                {
                    //The resource to lock on is there, so lock and run the methods and exit
                    await lockVersion.LockAndRunClass.LockAndRunMethodsAsync(scopedServices);
                    return true;
                }
                //else resource wasn't available so try another lock version
            }

            //Failed to find any resource to lock, so return a useful exception
            var listOfMissingResources = string.Join(Environment.NewLine,
                options.LockVersionsInOrder.Select(x => x.LockAndRunClass.ResourceName));
            throw new RunSequentiallyException(
                "No resource were found to lock, so could not run the registered services. The resources tried are" +
                Environment.NewLine + listOfMissingResources);
        }
    }
}