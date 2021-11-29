// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using RunMethodsSequentially;
using Test.EfCore;
using Test.Helpers;
using Test.ServicesToCall;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestFileSystemVersion
    {
        private readonly ITestOutputHelper _output;

        public TestFileSystemVersion(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestFileSystemLockRunOneService()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var lockAndRun = context.SetupFileSystemLockMethodsSequentially( 
                options => options.RegisterServiceToRunInJob<UpdateDatabase1>());

            //ATTEMPT
            using(new TimeThings(_output))
                await lockAndRun.LockAndLoadAsync();

            //VERIFY
            var entry = context.NameDateTimes.Single();
            entry.DateTimeUtc.ShouldBeInRange(DateTime.UtcNow.AddMilliseconds(-500), DateTime.UtcNow);
            entry.Name.ShouldEqual(nameof(UpdateDatabase1));
            var common = context.CommonNameDateTimes.Single();
            common.DateTimeUtc.ShouldEqual(entry.DateTimeUtc);
        }

        [Fact]
        public async Task TestFileSystemLockOrderedByWhatOrderToRunIn()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var lockAndRun = context.SetupNoLockRunMethodsSequentially(
                options =>
                {
                    options.RegisterServiceToRunInJob<UpdateWithZeroOrderNum>();
                    options.RegisterServiceToRunInJob<UpdateWithPositiveOrderNum>();
                    options.RegisterServiceToRunInJob<UpdateWithNegativeOrderNum>();
                });

            //ATTEMPT
            await lockAndRun.LockAndLoadAsync();

            //VERIFY
            var entries = context.NameDateTimes.OrderBy(x => x.Id).ToList();
            entries.Select(x => x.Name).ShouldEqual(new[] { "OrderNum = -1", "No OrderNum", "OrderNum = +1" });
        }
    }
}