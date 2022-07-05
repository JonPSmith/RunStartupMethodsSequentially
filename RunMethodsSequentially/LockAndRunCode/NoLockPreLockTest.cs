// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode;

/// <summary>
/// This is a <see cref="IPreLockTest"/> that will always say the resource is available.
/// This is useful of there only one instance of the application, as it quicker
/// </summary>
public class NoLockPreLockTest : IPreLockTest
{
    /// <summary>
    /// Returns true so that the <see cref="NoLockAndRunJob"/> can run the registered services / action
    /// </summary>
    /// <returns></returns>
    public ValueTask<bool> CheckLockResourceExistsAsync()
    {
        return new ValueTask<bool>(true);
    }

    /// <summary>
    /// Returns true so that the <see cref="NoLockAndRunJob"/> can run the registered services / action
    /// </summary>
    /// <returns></returns>
    public bool CheckLockResourceExists()
    {
        return true;
    }
}