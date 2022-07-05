// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode
{
    /// <summary>
    /// This defines the code that checks that the global resource you want to lock exists
    /// </summary>
    public interface IPreLockTest
    {
        /// <summary>
        /// This defines a method that will return true if the global resource exists
        /// </summary>
        /// <returns></returns>
        ValueTask<bool> CheckLockResourceExistsAsync();

        /// <summary>
        /// This defines a method that will return true if the global resource exists
        /// </summary>
        /// <returns></returns>
        bool CheckLockResourceExists();
    }
}