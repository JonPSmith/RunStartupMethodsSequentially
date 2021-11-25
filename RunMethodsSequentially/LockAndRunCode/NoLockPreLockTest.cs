﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode
{
    public class NoLockPreLockTest : IPreLockTest
    {
        public ValueTask<bool> CheckLockResourceExistsAsync()
        {
            return new ValueTask<bool>(true);
        }
    }
}