// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace RunMethodsSequentially
{
    /// <summary>
    /// This defines the service that you want run within a locked state
    /// </summary>
    public interface IServiceToCallWhileInLock
    {
        /// <summary>
        /// This method will be called within a lock state
        /// </summary>
        /// <returns></returns>
        ValueTask RunMethodWhileInLockAsync();
    }
}