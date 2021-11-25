// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This defines the code that will try to lock with the global resource
    /// and if the lock is successful it will then run your registered startup services within that lock
    /// </summary>
    public interface ILockAndRunJob
    {
        /// <summary>
        /// This contains the name of the resource that this version is looking for
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// This is the method that will lock the resource and then run the registered services
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        Task LockAndRunMethodsAsync(IServiceProvider serviceProvider);
    }
}