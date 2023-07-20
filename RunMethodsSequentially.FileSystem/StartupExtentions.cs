// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially.LockAndRunCode;

namespace RunMethodsSequentially
{
    public static class StartupExtensions
    {
        /// <summary>
        /// This will lock on a filesytem directory in your running application, e.g. 
        /// in ASP.NET Core the wwwroot directory. If it can't find the directory it will pass onto
        /// the next lock type. If there isn't a next lock type it will fail
        /// </summary>
        /// <param name="options"></param>
        /// <param name="directoryFilePath">The filepath to a global directory accessable by all the instances of your app</param>
        public static void AddFileSystemLockAndRunMethods(this RunSequentiallyOptions options,
            string directoryFilePath)
        {
            if (directoryFilePath == null) throw new ArgumentNullException(nameof(directoryFilePath));

            options.LockVersionsInOrder.Add(new TryLockVersion(
                new FileSystemDoesDirectoryExist(directoryFilePath),
                new FileSystemLockAndRunJob(options, directoryFilePath)));
        }
    }
}