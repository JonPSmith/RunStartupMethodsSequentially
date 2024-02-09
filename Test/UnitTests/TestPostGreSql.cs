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
            var options = this.CreatePostgreSqlUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(options);

            //ATTEMPT
            var connectionString = context.Database.GetConnectionString();

            //VERIFY
            connectionString.ShouldEqual("Host=127.0.0.1;Port=5432;Database=RunStartup-Test_TestPostgreSql;Username=postgres;Password=LetMeIn");
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

        [RunnableInDebugOnly]
        public void TestDeleteAllTestDatabases()
        {
            //SETUP

            //ATTEMPT
            using (new TimeThings(_output, "Deleted dbs"))
            {
                var numDeleted = DatabaseTidyHelper.DeleteAllPostgreSqlTestDatabases();
                _output.WriteLine($"Deleted {numDeleted} dbs");
            }

            //VERIFY
        }
    }
}