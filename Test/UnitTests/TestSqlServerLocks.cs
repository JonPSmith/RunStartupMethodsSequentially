﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
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
    public class TestSqlServerLocks
    {
        private readonly ITestOutputHelper _output;

        public TestSqlServerLocks(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ExampleRegisterRunMethodsSequentially()
        {
            //SETUP
            var connectionString = this.GetUniqueDatabaseConnectionString();
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterRunMethodsSequentially(options =>
            {
                options.AddSqlServerLockAndRunMethods(connectionString);
                options.AddFileSystemLockAndRunMethods(TestData.GetTestDataDir());
            }).RegisterServiceToRunInJob<UpdateDatabase1>()
            .RegisterServiceToRunInJob<UpdateDatabase2>();

            //VERIFY
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<RunSequentiallyOptions>();
            options.LockVersionsInOrder.Count.ShouldEqual(2);
            options.LockVersionsInOrder.First().LockAndRunClass.ResourceName.ShouldEqual(
                $"SQL Server database with name [{connectionString.GetDatabaseNameFromSqlServerConnectionString()}]");
            options.LockVersionsInOrder.Last().LockAndRunClass.ResourceName.ShouldEqual(
                @"Looking for directory at C:\Users\JonPSmith\source\repos\RunStartupMethodsSequentially\Test\TestData");
            serviceProvider.GetService<IHostedService>().ShouldNotBeNull();
        }

        [Fact]
        public void TestRegisterRunMethodsSequentiallyHostedService()
        {
            //SETUP
            var connectionString = this.GetUniqueDatabaseConnectionString();
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterRunMethodsSequentially(options =>
            {
                options.AddSqlServerLockAndRunMethods(connectionString);
            });

            //VERIFY
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<RunSequentiallyOptions>();
            options.LockVersionsInOrder.Count.ShouldEqual(1);
            options.LockVersionsInOrder.Single().LockAndRunClass.ResourceName.ShouldEqual(
                $"SQL Server database with name [{connectionString.GetDatabaseNameFromSqlServerConnectionString()}]");
            serviceProvider.GetService<IHostedService>().ShouldNotBeNull();
        }

        [Fact]
        public void TestRegisterRunMethodsSequentiallyNormalService()
        {
            //SETUP
            var connectionString = this.GetUniqueDatabaseConnectionString();
            var services = new ServiceCollection();

            //ATTEMPT
            services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                options.AddSqlServerLockAndRunMethods(connectionString);
            });

            //VERIFY
            var serviceProvider = services.BuildServiceProvider();
            var options = serviceProvider.GetRequiredService<RunSequentiallyOptions>();
            options.LockVersionsInOrder.Count.ShouldEqual(1);
            options.LockVersionsInOrder.Single().LockAndRunClass.ResourceName.ShouldEqual(
                $"SQL Server database with name [{connectionString.GetDatabaseNameFromSqlServerConnectionString()}]");
            serviceProvider.GetService<IGetLockAndThenRunServices>().ShouldNotBeNull();
        }

        [Fact]
        public async Task TestLockSqlDatabaseAndRunOneService()
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
            var entry = context.NameDateTimes.Single();
            entry.DateTimeUtc.ShouldBeInRange(DateTime.UtcNow.AddMilliseconds(-500), DateTime.UtcNow);
            entry.Name.ShouldEqual(nameof(UpdateDatabase1));
            var common = context.CommonNameDateTimes.Single();
            common.DateTimeUtc.ShouldEqual(entry.DateTimeUtc);
        }

        [Fact]
        public async Task TestLockSqlDatabaseAndRunTwoServices()
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
            var utcNow = DateTime.UtcNow;
            var entries = context.NameDateTimes.OrderBy(x => x.Id).ToList();
            entries[0].DateTimeUtc.ShouldBeInRange(DateTime.UtcNow.AddMilliseconds(-500), utcNow);
            entries[1].DateTimeUtc.ShouldBeInRange(entries[0].DateTimeUtc, utcNow);
            var common = context.CommonNameDateTimes.Single();
            common.DateTimeUtc.ShouldEqual(entries[1].DateTimeUtc);
        }


        [Fact]
        public async Task TestLockSqlDatabaseAndRunOrderedByWhatOrderToRunIn()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var services = context.SetupSqlServerRunMethodsSequentially(
                options =>
                {
                    options.RegisterServiceToRunInJob<UpdateWithPositiveOrderNum>();
                    options.RegisterServiceToRunInJob<UpdateWithZeroOrderNum>();
                    options.RegisterServiceToRunInJob<UpdateWithNegativeOrderNum>();
                });
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            await lockAndRun.LockAndLoadAsync();

            //VERIFY
            var entries = context.NameDateTimes.OrderBy(x => x.Id).ToList();
            entries.Select(x => x.Name).ShouldEqual(new[] { "OrderNum = -1", "No OrderNum", "OrderNum = +1" });
        }

        [Fact]
        public async Task TestLockSqlDatabaseNoDatabaseToStartWith()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureDeleted();

            var services = context.SetupSqlServerRunMethodsSequentially(
                options =>
                {
                    options.AddFileSystemLockAndRunMethods(TestData.GetTestDataDir());
                    options.RegisterServiceToRunInJob<SqlServerEnsureCreatedDatabaseOnly>();
                    options.RegisterServiceToRunInJob<UpdateDatabase1>();
                });
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            await lockAndRun.LockAndLoadAsync();

            //VERIFY
            var entry = context.NameDateTimes.Single();
            entry.DateTimeUtc.ShouldBeInRange(DateTime.UtcNow.AddMilliseconds(-500), DateTime.UtcNow);
            entry.Name.ShouldEqual(nameof(UpdateDatabase1));
            var common = context.CommonNameDateTimes.Single();
            common.DateTimeUtc.ShouldEqual(entry.DateTimeUtc);
        }

        //------------------------------------------------------
        //Check errors

        [Fact]
        public async Task TestLockSqlDatabase_NoDatabase()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureDeleted();

            var services = context.SetupSqlServerRunMethodsSequentially(
                options => options.RegisterServiceToRunInJob<UpdateDatabase1>());
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            var ex = await Assert.ThrowsAsync<RunSequentiallyException>(async () => await lockAndRun.LockAndLoadAsync());

            //VERIFY
            _output.WriteLine(ex.Message);
            ex.Message.ShouldStartWith("No resource were found to lock, so could not run the registered services. ");
        }

        [Fact]
        public async Task TestLockSqlDatabase_NoServicesFound()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var services = context.SetupSqlServerRunMethodsSequentially();
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            var ex = await Assert.ThrowsAsync<RunSequentiallyException>(async () => await lockAndRun.LockAndLoadAsync());

            //VERIFY
            _output.WriteLine(ex.Message);
            ex.Message.ShouldStartWith("You have not registered any services to run when the lock is active.");
        }

        [Fact]
        public async Task TestLockSqlDatabase_DuplicateServices()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var services = context.SetupSqlServerRunMethodsSequentially(
                options =>
                {
                    options.RegisterServiceToRunInJob<UpdateDatabase1>();
                    options.RegisterServiceToRunInJob<UpdateDatabase1>();
                });
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            var ex = await Assert.ThrowsAsync<RunSequentiallyException>(async () => await lockAndRun.LockAndLoadAsync());

            //VERIFY
            _output.WriteLine(ex.Message);
            ex.Message.ShouldStartWith("Some of your services registered by RegisterServiceToRunInJob<T> extension method are duplicates.");
        }

        [Fact]
        public async Task TestLockSqlDatabase_NoRegisteredLockService()
        {
            //SETUP
            var services = new ServiceCollection();
            services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                //options.AddLockSqlServerAndRunMethods(context.Database.GetConnectionString());
            });
            var testLogger = new RegisterTestLogger(services);
            var serviceProvider = services.BuildServiceProvider();
            var lockAndRun = serviceProvider.GetRequiredService<IGetLockAndThenRunServices>();

            //ATTEMPT
            var ex = await Assert.ThrowsAsync<RunSequentiallyException>(async () => await lockAndRun.LockAndLoadAsync());

            //VERIFY
            _output.WriteLine(ex.Message);
            ex.Message.ShouldStartWith("You must register at least one lock service when registering");
        }


        //--------------------------------------------------------------
        //private method


    }
}