// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RunMethodsSequentially.LockAndRunCode
{
    public class NoLockAndRunJob : ILockAndRunJob
    {
        public string ResourceName { get; } = "No locking applied";

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
        /// <exception cref="NotImplementedException"></exception>
        public ValueTask LockAndRunActionAsync(Func<ValueTask> actionAsync, RunSequentiallyOptions options)
        {
            return actionAsync();
        }

        public void LockAndRunAction(Action action, RunSequentiallyOptions options)
        {
            throw new NotImplementedException();
        }
    }
}