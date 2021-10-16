// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

namespace RunMethodsSequentially.LockAndRunCode
{
    public class TryLockVersion
    {
        public TryLockVersion(IPreLockTest preLockTask, ILockAndRunJob lockAndRunClass)
        {
            LockAndRunClass = lockAndRunClass;
            PreLockCheck = preLockTask;
        }

        public IPreLockTest PreLockCheck { get; }
        public ILockAndRunJob LockAndRunClass { get; }
    }
}