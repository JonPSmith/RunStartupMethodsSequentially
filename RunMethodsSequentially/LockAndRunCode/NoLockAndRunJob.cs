// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This is a <see cref="ILockAndRunJob"/> that will run the various services / action without obtaining a global lock.
    /// This is useful of there only one instance of the application, as it quicker
    /// </summary>
    public class NoLockAndRunJob : ILockAndRunJob
    {
        /// <summary>
        /// This contains the name of the resource that this version is looking for
        /// </summary>
        public string ResourceName { get; } = "No locking applied";

        /// <summary>
        /// This call the <see cref="JobRunner"/> to run all the
        /// registered <see cref="IStartupServiceToRunSequentially"/> services.
        /// It doesn't obtain a lock
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public async Task LockAndRunActionAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;      
            await scopedServices.RunJobAsync();
        }

        /// <summary>
        /// This runs the given async action
        /// </summary>
        /// <param name="actionAsync"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public ValueTask LockAndRunActionAsync(Func<ValueTask> actionAsync, RunSequentiallyOptions options)
        {
            return actionAsync();
        }

        /// <summary>
        /// This runs the given async action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public void LockAndRunAction(Action action, RunSequentiallyOptions options)
        {
            throw new NotImplementedException();
        }
    }
}