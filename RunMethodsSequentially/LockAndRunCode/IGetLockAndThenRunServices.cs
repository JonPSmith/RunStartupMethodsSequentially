// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// Interface that defines the service that is run on startup
    /// </summary>
    public interface IGetLockAndThenRunServices
    {
        /// <summary>
        /// This uses the PreLockCheck to check the the resource to be locked exists.
        /// If the resource does exist it calls the code to lock and run the methods.
        /// If no resource is found, then it throws an exception
        /// </summary>
        /// <returns>return true if successful</returns>
        Task<bool> LockAndLoadAsync();
    }
}