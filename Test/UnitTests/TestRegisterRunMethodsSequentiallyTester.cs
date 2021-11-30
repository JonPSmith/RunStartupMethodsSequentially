// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RunMethodsSequentially;
using Test.EfCore;
using Test.ServicesToCall;
using TestSupport.EfHelpers;
using WebSiteRunSequentially.Database;
using WebSiteRunSequentially.StartupServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests
{
    public class TestRegisterRunMethodsSequentiallyTester
    {
        [Fact]
        public async Task ExampleTester()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<WebSiteDbContext>();
            using var context = new WebSiteDbContext(dbOptions);
            context.Database.EnsureClean();

            var builder = new RegisterRunMethodsSequentiallyTester();

            //ATTEMPT
            //Copy your setup in your Program here 
            //---------------------------------------------------------------
            var connectionString = context.Database.GetConnectionString();  //CHANGED
            var lockFolder = builder.LockFolderPath;                        //CHANGED

            builder.Services.AddDbContext<WebSiteDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.RegisterRunMethodsSequentially(options =>
            {
                options.AddSqlServerLockAndRunMethods(connectionString);
                options.AddFileSystemLockAndRunMethods(lockFolder);
            })
                .RegisterServiceToRunInJob<StartupServiceEnsureCreated>()
                .RegisterServiceToRunInJob<StartupServiceSeedDatabase>();
            //----------------------------------------------------------------

            //VERIFY
            await builder.RunHostStartupCodeAsync();
            context.CommonNameDateTimes.Single().DateTimeUtc
                .ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow);
        }

        [Fact]
        public async Task TestRunTwoServiceOk()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var builder = new RegisterRunMethodsSequentiallyTester();

            //ATTEMPT
            //Copy your setup in your Program here 
            //---------------------------------------------------------------
            builder.Services.AddDbContext<TestDbContext>(options =>
                options.UseSqlServer(context.Database.GetConnectionString()));
            builder.Services.RegisterRunMethodsSequentially(options =>
            {
                options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
                options.AddFileSystemLockAndRunMethods(builder.LockFolderPath);
            })
                .RegisterServiceToRunInJob<UpdateDatabase1>()
                .RegisterServiceToRunInJob<UpdateDatabase2>();
            //----------------------------------------------------------------

            //VERIFY
            await builder.RunHostStartupCodeAsync();
        }

        [Fact]
        public async Task TestRunTwoServiceNoDatabaseOk()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureDeleted();

            var builder = new RegisterRunMethodsSequentiallyTester();

            //ATTEMPT
            //Copy your setup in your Program here 
            //---------------------------------------------------------------
            builder.Services.AddDbContext<TestDbContext>(options =>
                options.UseSqlServer(context.Database.GetConnectionString()));
            builder.Services.RegisterRunMethodsSequentially(options =>
            {
                options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
                options.AddFileSystemLockAndRunMethods(builder.LockFolderPath);
            })
                .RegisterServiceToRunInJob<SqlServerEnsureCreatedDatabaseOnly>()
                .RegisterServiceToRunInJob<UpdateDatabase2>();
            //----------------------------------------------------------------

            //VERIFY
            await builder.RunHostStartupCodeAsync();
        }

        [Fact]
        public async Task TestRunTwoServiceRegisterAsHostedServiceOk()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();

            var builder = new RegisterRunMethodsSequentiallyTester();

            //ATTEMPT
            //Copy your setup in your Program here 
            //---------------------------------------------------------------
            builder.Services.AddDbContext<TestDbContext>(options =>
                options.UseSqlServer(context.Database.GetConnectionString()));
            builder.Services.RegisterRunMethodsSequentially(options =>
            {
                options.RegisterAsHostedService = false;
                options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
                options.AddFileSystemLockAndRunMethods(builder.LockFolderPath);
            })
                .RegisterServiceToRunInJob<UpdateDatabase1>()
                .RegisterServiceToRunInJob<UpdateDatabase2>();
            //----------------------------------------------------------------

            //VERIFY
            await builder.RunHostStartupCodeAsync();
        }

        [Fact]
        public async Task TestRunTwoServiceThrowsException()
        {
            //SETUP
            var dbOptions = this.CreateUniqueClassOptions<TestDbContext>();
            using var context = new TestDbContext(dbOptions);
            context.Database.EnsureClean();
            var lockFolder = System.IO.Directory.GetCurrentDirectory();

            var builder = new RegisterRunMethodsSequentiallyTester();
            //Copy your setup in your Program here 
            //---------------------------------------------------------------
            builder.Services.AddDbContext<TestDbContext>(options =>
                options.UseSqlServer(context.Database.GetConnectionString()));
            builder.Services.RegisterRunMethodsSequentially(options =>
            {
                options.AddSqlServerLockAndRunMethods(context.Database.GetConnectionString());
                options.AddFileSystemLockAndRunMethods(builder.LockFolderPath);
            })
                .RegisterServiceToRunInJob<UpdateDatabase1>()
                .RegisterServiceToRunInJob<UpdateDatabase1>();  //!!!!!!!!!!!!!!!!!! DUPLICATE !!!!!!!!!!!!!!!
            //----------------------------------------------------------------

            //ATTEMPT
            var ex = await Assert.ThrowsAsync< RunSequentiallyException>(async () => await builder.RunHostStartupCodeAsync());

            //VERIFY
            ex.Message.ShouldEqual("Some of your services registered by RegisterServiceToRunInJob<T> extension method are duplicates. They are: UpdateDatabase1");
        }

    }
}