// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace RunMethodsSequentially.LockAndRunCode;

/// <summary>
/// This class holds the two parts to get a global lock
/// </summary>
public class TryLockVersion
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="preLockTask"></param>
    /// <param name="lockAndRunClass"></param>
    public TryLockVersion(IPreLockTest preLockTask, ILockAndRunJob lockAndRunClass)
    {
        LockAndRunClass = lockAndRunClass;
        PreLockCheck = preLockTask;
    }

    /// <summary>
    /// Holds the class to check if the global resource is available
    /// </summary>
    public IPreLockTest PreLockCheck { get; }

    /// <summary>
    /// This th class that will lock the global resource and then run the services / action
    /// </summary>
    public ILockAndRunJob LockAndRunClass { get; }
}