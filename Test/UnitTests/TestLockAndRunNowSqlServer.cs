// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially;
using RunMethodsSequentially.LockAndRunCode;
using Test.EfCore;
using Test.Helpers;
using Test.ServicesToCall;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestLockAndRunNowSqlServer
{
    [Fact]
    public async Task TestRunActionAsync()
    {
        //SETUP
        var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
        using var context = new TestDbContext(dbOptions);
        context.Database.EnsureClean();

        var hasRun = false;
        ValueTask MyAction()
        {
            hasRun = true;
            return ValueTask.CompletedTask;
        }
        
        //ATTEMPT
        await LockAndRunNow.RunActionAsync(MyAction, options =>
        {
            options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
        });

        //VERIFY
        hasRun.ShouldBeTrue();
    }
}