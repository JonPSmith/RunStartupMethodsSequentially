// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using RunMethodsSequentially.LockAndRunCode;
using Test.EfCore;
using Test.Helpers;
using TestSupport.Attributes;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestPostgreSql
    {
        private readonly ITestOutputHelper _output;

        public TestPostgreSql(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreatePostgreUniqueDatabaseOptions()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<TestDbContext>();
            using var context = new TestDbContext(options);

            //ATTEMPT
            var connectionString = context.Database.GetConnectionString();

            //VERIFY
            connectionString.ShouldEqual("Host=localhost;Database=DatabaseTest_TestPostgreSql;Username=postgres;Password=LetMeIn");
        }

        [Fact]
        public async Task TestDeleteCreateDatabaseEfCore()
        {
            //SETUP
            var logs = new List<string>();
            var options = this.CreatePostgreSqlUniqueClassOptionsWithLogTo<TestDbContext>(log => logs.Add(log));
            using var context = new TestDbContext(options);
            using(new TimeThings(_output, "Possible delete db"))
                context.Database.EnsureDeleted();

            var service = new PostgreSqlDoesDatabaseExist(context.Database.GetConnectionString());

            //ATTEMPT
            var noDb = await service.CheckLockResourceExistsAsync();
            using (new TimeThings(_output, "Create db"))
                context.Database.EnsureCreated();
            var hasDb = await service.CheckLockResourceExistsAsync();
        
            //VERIFY
            noDb.ShouldBeFalse();
            hasDb.ShouldBeTrue();
        }


        [Fact]
        public async Task TestResetDatabaseUsingRespawn()
        {
            //SETUP
            var options = this.CreatePostgreSqlUniqueDatabaseOptions<TestDbContext>();
            using var context = new TestDbContext(options);
            context.Database.EnsureCreated();

            context.Add(new NameDateTime { Name = "test" });
            context.SaveChanges();
            context.ChangeTracker.Clear();

            //ATTEMPT
            using (new TimeThings(_output, "wipe database using respawn"))
                await context.EnsureCreatedAndEmptyPostgreSqlAsync();

            //VERIFY
            (await context.NameDateTimes.CountAsync()).ShouldEqual(0);
        }

        [RunnableInDebugOnly]
        public void TestDeleteAllTestDatabases()
        {
            //SETUP

            //ATTEMPT
            using (new TimeThings(_output, "Deleted dbs"))
            {
                var numDeleted = PostgreSqlHelpers.DeleteAllPostgreSqlUnitTestDatabases();
                _output.WriteLine($"Deleted {numDeleted} dbs");
            }

            //VERIFY
        }
    }
}