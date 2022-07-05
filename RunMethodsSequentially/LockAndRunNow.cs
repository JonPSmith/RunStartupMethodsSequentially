// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace RunMethodsSequentially;

/// <summary>
/// 
/// </summary>
public static class LockAndRunNow
{
    public static async ValueTask RunActionAsync(Func<ValueTask> yourActionAsync, Action<RunSequentiallyOptions> optionsAction = null)
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



    public static void RunAction(Action yourAction, Action<RunSequentiallyOptions> optionsAction = null)
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