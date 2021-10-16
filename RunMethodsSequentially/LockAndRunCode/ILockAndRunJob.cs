// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode
{
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