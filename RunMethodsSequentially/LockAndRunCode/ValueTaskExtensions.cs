// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace RunMethodsSequentially.LockAndRunCode;

/// <summary>
/// see https://www.thereformedprogrammer.net/using-valuetask-to-create-methods-that-can-work-as-sync-or-async/
/// </summary>
public static class ValueTaskExtensions
{
    /// <summary>
    /// This will check the <see cref="ValueTask"/> returned
    /// by a method and ensure it didn't run any async methods.
    /// It then calls GetAwaiter().GetResult() which will
    /// bubble up an exception if there is one
    /// </summary>
    /// <param name="valueTask">The ValueTask from a method that didn't call any async methods</param>
    public static void CheckSyncValueTaskWorked(this ValueTask valueTask)
    {
        if (!valueTask.IsCompleted)
            throw new InvalidOperationException("Expected a sync task, but got an async task");
        //Stephen Toub recommended calling GetResult every time.
        //This helps with pooled resources, that use the GetResult call to tell it has finished being used
        valueTask.GetAwaiter().GetResult();
    }

    /// <summary>
    /// This will check the <see cref="ValueTask{TResult}"/> returned
    /// by a method and ensure it didn't run any async methods.
    /// It then calls GetAwaiter().GetResult() to return the result
    /// Calling .GetResult() will also bubble up an exception if there is one
    /// </summary>
    /// <param name="valueTask">The ValueTask from a method that didn't call any async methods</param>
    /// <returns>The result returned by the method</returns>
    public static TResult CheckSyncValueTaskWorkedAndReturnResult<TResult>(this ValueTask<TResult> valueTask)
    {
        if (!valueTask.IsCompleted)
            throw new InvalidOperationException("Expected a sync task, but got an async task");
        return valueTask.GetAwaiter().GetResult();
    }
}