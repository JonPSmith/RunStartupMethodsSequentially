// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RunMethodsSequentially;
using RunMethodsSequentially.LockAndRunCode;
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
    public class TestLogging
    {
        private readonly ITestOutputHelper _output;

        public TestLogging(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestLoggingRunOneService()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var services = context.SetupSqlServerRunMethodsSequentially( 
                options => options.RegisterServiceToRunInJob<UpdateDatabase1>());
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            using (new TimeThings(_output))
                await lockAndRun.LockAndLoadAsync();

            //VERIFY
            foreach(var log in testLogger.Logs)
            {
                _output.WriteLine(log.Message);
            }
            testLogger.Logs.Count.ShouldEqual(2);
            var i = 0;
            testLogger.Logs[i++].Message.ShouldEqual("The SQL Server database with name [RunMethodsSequentially-Test_TestLogging] exists and will be locked.");
            testLogger.Logs[i++].Message.ShouldEqual("The startup service class [UpdateDatabase1] was successfully executed.");
        }

        [Fact]
        public async Task TestLoggingRunTwoServices()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var services = context.SetupSqlServerRunMethodsSequentially(
                options =>
                {
                    options.RegisterServiceToRunInJob<UpdateDatabase1>();
                    options.RegisterServiceToRunInJob<UpdateDatabase2>();
                });
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            await lockAndRun.LockAndLoadAsync();

            //VERIFY
            foreach (var log in testLogger.Logs)
            {
                _output.WriteLine(log.Message);
            }
            testLogger.Logs.Count.ShouldEqual(3);
            var i = 0;
            testLogger.Logs[i++].Message.ShouldEqual("The SQL Server database with name [RunMethodsSequentially-Test_TestLogging] exists and will be locked.");
            testLogger.Logs[i++].Message.ShouldEqual("The startup service class [UpdateDatabase1] was successfully executed.");
            testLogger.Logs[i++].Message.ShouldEqual("The startup service class [UpdateDatabase2] was successfully executed.");
        }

    }
}