// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RunMethodsSequentially.LockAndRunCode;
using Test.EfCore;
using Test.Helpers;
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
        public void TestCreatePostGreUniqueDatabaseOptions()
        {
            //SETUP
            var options = this.CreatePostGreUniqueDatabaseOptions<TestDbContext>();
            using var context = new TestDbContext(options);

            //ATTEMPT
            var connectionString = context.Database.GetConnectionString();

            //VERIFY
            connectionString.ShouldEqual("Host=localhost;Database=DatabaseTest_TestPostGreSql;Username=postgres;Password=LetMeIn");
        }

        [Fact]
        public async Task TestDeleteCreateDatabase()
        {
            //SETUP
            var options = this.CreatePostGreUniqueDatabaseOptions<TestDbContext>();
            using var context = new TestDbContext(options);
            context.Database.EnsureDeleted();

            var service = new PostgreSqlDoesDatabaseExist(context.Database.GetConnectionString());

            //ATTEMPT
            var noDb = await service.CheckLockResourceExistsAsync();
            context.Database.EnsureCreated();
            var hasDb = await service.CheckLockResourceExistsAsync();
        
            //VERIFY
            noDb.ShouldBeFalse();
            hasDb.ShouldBeTrue();
        }
    }
}