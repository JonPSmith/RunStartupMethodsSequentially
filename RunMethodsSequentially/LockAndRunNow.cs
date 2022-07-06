// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace RunMethodsSequentially;

/// <summary>
/// This class allows you to obtain a lock on a global resource and then runs the provided action
/// </summary>
public static class LockAndRunNow
{
    /// <summary>
    /// This will obtain a lock on a global resource and then run your async action
    /// </summary>
    /// <param name="yourActionAsync"></param>
    /// <param name="optionsAction"></param>
    /// <returns></returns>
    public static async ValueTask RunActionInLockAsync(Func<ValueTask> yourActionAsync, Action<RunSequentiallyOptions> optionsAction = null)
    {
        var options = new RunSequentiallyOptions(null);
        optionsAction?.Invoke(options);

        foreach (var lockVersion in options.LockVersionsInOrder)
        {
            if (await lockVersion.PreLockCheck.CheckLockResourceExistsAsync())
            {
                //The resource to lock on is there, so lock and run the methods and exit
                await lockVersion.LockAndRunClass.LockAndRunActionAsync(yourActionAsync, options);
                return;
            }
        }
        //Failed to find any resource to lock, so return a useful exception
        ThrowExceptionMissingResources(options);
    }

    /// <summary>
    /// This will obtain a lock on a global resource and then run your sync action
    /// </summary>
    /// <param name="yourAction"></param>
    /// <param name="optionsAction"></param>
    public static void RunActionInLock(Action yourAction, Action<RunSequentiallyOptions> optionsAction = null)
    {
        var options = new RunSequentiallyOptions(null);
        optionsAction?.Invoke(options);

        foreach (var lockVersion in options.LockVersionsInOrder)
        {
            if (lockVersion.PreLockCheck.CheckLockResourceExists())
            {
                //The resource to lock on is there, so lock and run the methods and exit
                lockVersion.LockAndRunClass.LockAndRunAction(yourAction, options);
                return;
            }
        }
        //Failed to find any resource to lock, so return a useful exception
        ThrowExceptionMissingResources(options);
    }

    //-------------------------------------------------
    //private methods

    private static void ThrowExceptionMissingResources(RunSequentiallyOptions options)
    {
        var listOfMissingResources = string.Join(Environment.NewLine,
            options.LockVersionsInOrder.Select(x => x.LockAndRunClass.ResourceName));
        throw new RunSequentiallyException(
            "No resource were found to lock, so could not run the action. The resources tried are" +
            Environment.NewLine + listOfMissingResources);
    }
}