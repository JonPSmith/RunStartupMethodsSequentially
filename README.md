# RunStartupMethodsSequentially

This .NET library is designed to updates to single resources, e.g. a database, on startup of an application that has multiple instances running (e.g. a ASP.NET Core web site with _Azure Scale Out_ is turned on). This library runs when the application starts up and run your _startup services_. Typical startup services are: migrating to a database, seeding a database and/or ensuring that an admin user has been added. It can also be used with non-database resources as Azure blob, common files, etc.

> **Scale Out**: A scale out operation is the equivalent of creating multiple copies of your web site and adding a load balancer to distribute the demand  between them (taken from [Microsoft Azure docs](https://azure.microsoft.com/en-gb/blog/scaling-up-and-scaling-out-in-windows-azure-web-sites/)).

This open-source library available on NuGet as [Net.RunMethodsSequentially](https://www.nuget.org/packages/Net.RunMethodsSequentially). The documentation can be found in the README file and see the [Release notes](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/ReleaseNotes.md) for details of changes.

## How it works, and what to watch out for

This library allows you create services known as _startup services_ which are run sequentially on startup within a [DistributedLock](https://github.com/madelson/DistributedLock) global lock. This global lock stops the problem of multiple instances of the application trying to update one common resource at the same time. For instance EF Core's Migrate feature doesn't work if multiple migrations are applied at the same time.

But be aware every startup service will be run on every application's instances, for example if your application is running four instances then your startup service will be run four times. This means your startup services should check if the database has already been updated, e.g. if your service adds an admin user to the the authentication database it should first check that that admin user isn't already been added (NOTE: EF Core's `Migrate` method checks if the database needs to be updated, which stops your database being migrated multiple times).

Secondly, you should test your use of this library because in ASP.NET Core it is run as a [_hosted service_](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) and if there is an exception the application won't give you any feedback on what went wrong. I have added a class called []

Another warning is that applying a database migration is that the migration must not contain what are known as _breaking changes_ - that is a change that will cause a the currently running application from working (see [the five-stage app update in this article](https://www.thereformedprogrammer.net/handling-entity-framework-core-database-migrations-in-production-part-2/) for more info). This issue isn't specific to this library, but applies whenever you are updating an application without any break in the service, known as _24/7_ or _continuous_ application.

## What you have to do to use this library?

1. Add the [Net.RunMethodsSequentially](https://www.nuget.org/packages/Net.RunMethodsSequentially) NuGet package to your main application.
2. Create your _startup services_ that will update a common resource, e.g. Migrating a database. Each startup service must implement the RunMethodsSequentially's [IStartupServiceToRunSequentially](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/IStartupServiceToRunSequentially.cs) interface.
3. Register the `RegisterRunMethodsSequentially` extension method to your dependency injection provider, and select:
   - What global resource(s) you want to lock on.
   - Register your startup service(s) you want run on startup.

If you are working with ASP.NET Core, then that is all you need to do. If you are working with another approach, like a [Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) you have to manually call the `IGetLockAndThenRunServices` service (see `RegisterAsHostedService` in the [Extra options](https://github.com/JonPSmith/RunStartupMethodsSequentially#extra-options) section).


## 1. Add NuGet Net.RunMethodsSequentially to your app

Simply add the [Net.RunMethodsSequentially](https://www.nuget.org/packages/Net.RunMethodsSequentially) NuGet package to your application. This NuGet library is needed in the project holding your startup services, and the project containing the an ASP.NET Core Program class for registering the library.

## 2. Creating your startup services

To create a startup service that will be run while in a lock you need to create a class that inherits the [IStartupServiceToRunSequentially](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/IStartupServiceToRunSequentially.cs) interface. This has a method defined has a `ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)`. This method is where you put your code to update a shared resource, such as a database.

Here is an example of a startup service, in this case its a EF Core Migrate of a database.

```c#
public class MigrateDbContextService : 
    IStartupServiceToRunSequentially
{
    public int OrderNum { get; }

    public async ValueTask ApplyYourChangeAsync(
        IServiceProvider scopedServices)
    {
        var context = scopedServices
            .GetRequiredService<TestDbContext>();

        //NOTE: The Migrate method will only update 
        //the database if there any new migrations to add
        await context.Database.MigrateAsync();
    }
}
```

The `ApplyYourChangeAsync` is given a _scoped service provider_, which means its copy of the normal services used in the main application. From this you can get the services you need to apply your changes to the shared resource, in this case a database.

_NOTE: You can use the normal constructor DI injection, but as a scoped service provider is already available you can use that instead._

The `OrderNum` is a way to define the order you want your startup services are run. If services have the same `OrderNum` value, like zero, they will be run in the order they were registered (see next section).

I want to emphasize that your startup services are going to be run multiple times, once for each instance of your application. Therefore your startup service must check if the update it wants to do hasn't already been applies. EF Core's `MigrateAsync` automatically checks if the database needs to be updated, but if you are seeding a database then you check that the seeded hasn't already been done. 

Here is a example of adding a admin user to the individual account authentication database, which checks if the admin user hasn't already been added - see test at the bottom of the code.

```c#
public class IndividualAccountsAddSuperUser : IStartupServiceToRunSequentially
{
    public int OrderNum { get; }

    public async ValueTask ApplyYourChangeAsync(IServiceProvider scopedServices)
    {
        var context = scopedServices
            .GetRequiredService<UserManager<TIdentityUser>>();
        var config = scopedServices
            .GetRequiredService<IConfiguration>();
        var superSection = config.GetSection("SuperAdmin");

        var user = await userManager
            .FindByEmailAsync(superSection["Email"]);

        if (user != null)
            return; //ALREADY SO DON'T TRY TO ADD AGAIN

        //... code to add a new user left out
    }
}
``` 

## 3. Register RunMethodsSequentially your application

A typical configuration setup for the RunMethodsSequentially feature depends on what your application is:

### For ASP.NET Core

You most likely want to get the database connection string held in the appsettings.json file using the `IConfiguration` service and an known folder, either the `IWebHostEnvironment`'s  `ContentRootPath` or `WebRootPath`. The code below shows how to do this in the net6.0 `Program` class

```c#
using RunMethodsSequentially;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");
var lockFolder = builder.Environment.WebRootPath;

builder.Services.RegisterRunMethodsSequentially(options =>
    {
        options.AddSqlServerLockAndRunMethods(connectionString));
        options.AddFileSystemLockAndRunMethods(lockFolder);
    })
    .RegisterServiceToRunInJob<MigrateDatabaseOnStartup>()
    .RegisterServiceToRunInJob<SeedDatabaseOnStartup>();

//... other setup code left out
```

_NOTE: The RunStartupMethodsSequentially repo contains a ASP.NET Core application called WebSiteRunSequentially that I used to check that it worked on Azure with scale out. The [Program class](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/WebSiteRunSequentially/Program.cs)._

### For non-ASP.NET Core applications

For applications where you don't have access to the  you will need to rely on a constant for your connectionString and static methods such as `AppContext.BaseDirectory` or `Environment.CurrentDirectory` to get a folder.

Also, you need to set the `RegisterAsHostedService`, which will register as a `IGetLockAndThenRunServices` transient service instead of the the ASP.NET Core specific `HostedService`.

```c#
services.RegisterRunMethodsSequentially(options =>
    {
        options.RegisterAsHostedService = false;
        options.AddSqlServerLockAndRunMethods(connectionString));
        options.AddFileSystemLockAndRunMethods(
            Environment.CurrentDirectory);
    })
.RegisterServiceToRunInJob<MigrateDatabaseOnStartup>()
.RegisterServiceToRunInJob<SeedDatabaseOnStartup>();
```

You then need to run the `IGetLockAndThenRunServices` transient service and call its `LockAndLoadAsync` method.

## Explanation of the various methods

The following subsections describe each part:

### 3.a `RegisterRunMethodsSequentially` extension method

This method allows you to register one or more _LockAndRun_ methods. Each LockAndRun method first checks that the provided resource type exists. If the resource isn't found, then it exits and allows later LockAndRun methods to try to get a lock on a resource. If no resource can be found to create a global lock, the it will throw an exception.

There are four LockAndRun methods (and its not hard to create others):

- `AddSqlServerLockAndRunMethods(connectionString)`, which works with a SQL Server database
- `AddPostgreSqlLockAndRunMethods(connectionString)`, which is for a PostGreSQL database
- `AddFileSystemLockAndRunMethods(- path to global directory -)`, which uses a FileSystem directory shared across all of the application's instances.
- `AddRunMethodsWithoutLock()` which doesn't lock anything and just runs your startup services. This gives you a way to turn off the lock if you don't have multiple instances (locking a local SQL Server database takes 1 ms, but no lock only takes 50 us.)

_NOTE: It is fairly easy to add another LockAndRun methods as long as the [DistributedLock](https://github.com/madelson/DistributedLock) has a lock version for a global lock available to your application. So if you want to lock on Redis, Azure blob etc. then you can write the extra lock code yourself - just follow the pattern of the existing LockAndRun code._

The reason for having a series of LockAndRun methods is to allow for resources that might not created yet. For instance, if the database doesn't currently exist, then it can't obtain a lock on the database. In this case the second FileSystem LockAndRun method can obtain a lock on a FileSystem directory that all the applications can access. 

The first LockAndRun method that obtains a global lock will then run all your startup services you have registered to RunMethodsSequentially (see `RegisterServiceToRunInJob<T>` section later). Once the your startup services have run on one application instance it is on, then it releases the global lock, which allows another instance of the application to run until all the instances have been run.

Of course, once one instance has successfully applied your startup services to the global resource, then other instances are still going to run. That is why your startup services must check if the update they are planning to add haven't already been applied.

#### Extra options

The `options` also has default settings for some of the code, but you can override these. They are:

- `options.RegisterAsHostedService`: By default this is true, and the the `IGetLockAndThenRunServices` service isn't registered, but the [GetLockAndThenRunHostedService](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/LockAndRunCode/GetLockAndThenRunHostedService.cs) is registered as a `HostedService`. In ASP.NET Core that means the `IGetLockAndThenRunServices` code is run on startup.
- `options.DefaultLockTimeoutInSeconds`: By default this is set to 100 seconds, and defines the time it will wait for a lock can be acquired. _NOTE: When you have `NNN` multiple instances the time for the ALL of your startup services must be less that `DefaultLockTimeoutInSeconds / NNN`.
- There are other settings - see [RunSequentiallyOptions](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/RunSequentiallyOptions.cs) for full details.



### 3.b Registering your start services

The code below just shows just the registering of the RunMethodsSequentially code, with the last two lines using the `RegisterServiceToRunInJob<T>` extension method to register your startup services. In this case its two startup services, but it can be many more (_NOTE: see the 2. Creating your startup services section about the order in which your startup services are run_).

```c#
services.RegisterRunMethodsSequentially(options =>
    {
        options.AddSqlServerLockAndRunMethods(connectionString));
        options.AddFileSystemLockAndRunMethods(lockFolder);
    })
    .RegisterServiceToRunInJob<MigrateDatabaseOnStartup>()
    .RegisterServiceToRunInJob<SeedDatabaseOnStartup>();
```

There are two rules about your startup services:

1. They must inherit the [`IStartupServiceToRunSequentially`](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/IStartupServiceToRunSequentially.cs) interface.
2. Your startup services most likely have to be run in a certain order, for instance you should Migrate/Create your database startup service before any startup services that access that database. You have two options:
   1. The simplest approach is you register your services in the correct order using the `RegisterServiceToRunInJob<T>` method.
   2. If you have a complex situation (like the one in my [`AuthPermissions.AspNetCore`](https://github.com/JonPSmith/AuthPermissions.AspNetCore) library) then you can use the `OrderNum` in your startup service to define the order.

[End]
