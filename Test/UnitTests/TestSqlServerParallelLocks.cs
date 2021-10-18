// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Medallion.Threading.FileSystem;
using Medallion.Threading.SqlServer;
using Microsoft.EntityFrameworkCore;
using RunMethodsSequentially;
using Test.EfCore;
using Test.Helpers;
using Test.ServicesToCall;
using TestSupport.EfHelpers;
using TestSupport.Helpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{

    //NOTE Parallel.ForEach doesn't handle async
    //I found a very useful article https://medium.com/@alex.puiu/parallel-foreach-async-in-c-36756f8ebe62 
    //From this I decided the AsyncParallelForEach approach, which can run async methods in parallel
    public class TestSqlServerParallelLocks
    {
        private readonly ITestOutputHelper _output;

        public TestSqlServerParallelLocks(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestAsyncParallelForEachNoLocking()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var logs = new ConcurrentStack<string>();

            async Task TaskAsync(int num)
            {
                logs.Push($"S{num}: {DateTime.UtcNow:O}");
                await Task.Delay(100);
                logs.Push($"E{num}: {DateTime.UtcNow:O}");
            }

            //ATTEMPT
            await 3.NumTimesAsyncEnumerable().AsyncParallelForEach(TaskAsync);

            //VERIFY
            foreach (var log in logs.Reverse())
            {
                _output.WriteLine(log);
            }
            logs.Reverse().Select(x => x.First()).ShouldEqual(new []{'S', 'S', 'S', 'E', 'E', 'E', });
        }

        [Fact]
        public async Task TestAsyncParallelForEachWithSqlLocking()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var logs = new ConcurrentStack<string>();

            async Task TaskAsync(int i)
            {
                var distributedLock = new SqlDistributedLock("MyLock", context.Database.GetConnectionString());
                await using (await distributedLock.AcquireAsync())
                {
                    logs.Push($"S{i}: {DateTime.UtcNow:O}");
                    await Task.Delay(100);
                    logs.Push($"E{i}: {DateTime.UtcNow:O}");
                }
            }

            //ATTEMPT
            await 3.NumTimesAsyncEnumerable().AsyncParallelForEach(TaskAsync);

            //VERIFY
            foreach (var log in logs.Reverse())
            {
                _output.WriteLine(log);
            }
            logs.Reverse().Select(x => x.First()).ShouldEqual(new[] { 'S', 'E', 'S', 'E', 'S', 'E', });
        }

        [Fact]
        public async Task TestAsyncParallelForEachWithFileSystemLocking()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var logs = new ConcurrentStack<string>();

            async Task<int> TaskAsync(int i)
            {
                var lockFileDirectory = new DirectoryInfo(TestData.GetTestDataDir());
                var distributedLock = new FileDistributedLock(lockFileDirectory, "MyLock");
                await using (await distributedLock.AcquireAsync())
                {
                    logs.Push($"S{i}: {DateTime.UtcNow:O}");
                    await Task.Delay(100);
                    logs.Push($"E{i}: {DateTime.UtcNow:O}");
                }

                return i;
            }

            //ATTEMPT
            await 3.NumTimesAsyncEnumerable().AsyncParallelForEach(TaskAsync);

            //VERIFY
            foreach (var log in logs.Reverse())
            {
                _output.WriteLine(log);
            }
            logs.Reverse().Select(x => x.First()).ShouldEqual(new[] { 'S', 'E', 'S', 'E', 'S', 'E', });
        }

        [Fact]
        public async Task TestLockSqlDatabaseAndRunOneService()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            context.Add(new CommonNameDateTime { Name = "start", DateTimeUtc = DateTime.UtcNow });
            await context.SaveChangesAsync();

            async Task TaskAsync(int i1)
            {
                using var localContext = new TestDbContext(dbOptions);
                var lockAndRun = localContext.SetupRunMethodsSequentially(
                    options => options.RegisterServiceToRunInJob<UpdateWithDelay>());
                var success = await lockAndRun.LockAndLoadAsync();
            }

            //ATTEMPT
            await 3.NumTimesAsyncEnumerable().AsyncParallelForEach(TaskAsync);

            //VERIFY
            var entities = context.NameDateTimes.ToList();
            foreach (var entity in entities)
            {
                _output.WriteLine(entity.ToString());
            }
            entities.Select(x => x.Name).ShouldEqual(new[] { "Read", "Write", "Read", "Write", "Read", "Write", });
            for (int i = 1; i < entities.Count; i++)
            {
                (entities[i].DateTimeUtc >= entities[i - 1].DateTimeUtc).ShouldBeTrue(i.ToString());
            }
        }
    }
}