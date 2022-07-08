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

        //ATTEMPT
        LockAndRunNow.RunActionInLock(() => { hasRun = true; }, options =>
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

        //ATTEMPT
        await LockAndRunNow.RunActionInLockAsync(() => { hasRun = true;
            return ValueTask.CompletedTask;
        }, options =>
        {
            options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
        });

        //VERIFY
        hasRun.ShouldBeTrue();
    }
}