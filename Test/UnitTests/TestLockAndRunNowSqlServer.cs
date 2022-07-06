// Copyright (c) 2022 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RunMethodsSequentially;
using Test.EfCore;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests;

public class TestLockAndRunNowSqlServer
{
    [Fact]
    public void TestRunAction()
    {
        //SETUP
        var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
        using var context = new TestDbContext(dbOptions);
        context.Database.EnsureClean();

        var hasRun = false;
        void MyAction()
        {
            hasRun = true;
        }

        //ATTEMPT
        LockAndRunNow.RunActionInLock(MyAction, options =>
        {
            options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
        });

        //VERIFY
        hasRun.ShouldBeTrue();
    }

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
        await LockAndRunNow.RunActionInLockAsync(MyAction, options =>
        {
            options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
        });

        //VERIFY
        hasRun.ShouldBeTrue();
    }
}