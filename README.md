# RunStartupMethodsSequentially

This .NET library is designed to handle updates to single resources, e.g. a database, on startup of an application that has multiple instances running (e.g. a ASP.NET Core web site with _Azure Scale Out_ is turned on). This library runs when the application starts up and run your _startup services_. Typical startup services are: migrating to a database, seeding a database and/or ensuring that an admin user has been added. It can also be used with non-database resources as Azure blob, common files, etc.

> **Scale Out**: A scale out operation is the equivalent of creating multiple copies of your web site and adding a load balancer to distribute the demand  between them (taken from [Microsoft Azure docs](https://azure.microsoft.com/en-gb/blog/scaling-up-and-scaling-out-in-windows-azure-web-sites/)).

This open-source library available on NuGet (!!! not yet). The documentation can be found in this README file and see the [Release notes](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/ReleaseNotes.md) for details of changes.

## How it works, and what to watch out for

This library allows you create services known as _startup services_ which are run sequentially on startup within a [DistributedLock](https://github.com/madelson/DistributedLock) global lock. This global lock stops the problem of multiple instances of the application trying to update one common resource at the same time. For instance  EF Core's Migrate feature doesn't work if multiple migrations are applied at the same time.

But be aware every startup service will be run on every application's instances, which means your startup services should check if the database has already been updated, e.g. if your service adds an admin user to the the authentication database it should first check that that admin user isn't already been added.

Another warning is that applying a database migration is that the migration must not contain what are known as _breaking changes_ - that is a change that will cause a the currently running application from working (see [the five-stage app update in this article](https://www.thereformedprogrammer.net/handling-entity-framework-core-database-migrations-in-production-part-2/) for more info). This issue isn't specific to this library, but applies whenever you are updating an application without any break in the service, known as _24/7_ or _continuous_ application.

## What you have to do to use this library?

1. Add the RunStartupMethodsSequentially NuGet package (!!! not out yet) to your main application, typically an ASP.NET Core project
2. Create some _startup services_ that will update a common resource, e.g. Migrating a database. Each startup service must implement the RunMethodsSequentially's [IServiceToCallWhileInLock](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/IServiceToCallWhileInLock.cs) interface.
3. Register the RunMethodsSequentially library to your dependency injection provider, and select:
   - What global resource(s) you want to lock on.
   - Register your startup service(s) you want run on startup.

If you are working with ASP.NET Core, then that is all you need to do. If you are working with another approach, like a [Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) you have to manually call the `IGetLockAndThenRunServices` service (see `RegisterAsHostedService` in the [Extra options](https://github.com/JonPSmith/RunStartupMethodsSequentially#extra-options) section).

## Configuring RunMethodsSequentially in ASP.NET Core

A typical configuration setup for the RunMethodsSequentially feature would look like this:

```c#
services.RegisterRunMethodsSequentially(options =>
    {
        options.AddSqlServerLockAndRunMethods(connectionString));
        options.AddFileSystemLockAndRunMethods(Environment.CurrentDirectory);
    })
.RegisterServiceToRunInJob<MigrateDatabaseOnStartup>()
.RegisterServiceToRunInJob<SeedDatabaseOnStartup>();
```

The following subsections describe each part:

### `RegisterRunMethodsSequentially` extension method

This method allows you to register one or more _LockAndRun_ methods. Each LockAndRun method first checks that the provided resource type exists. If the resource isn't found, then it exits and allows later LockAndRun methods to try to get a lock on a resource. If no resource can be found to create a global lock, the it will throw an exception.

There are three LockAndRun methods (and its not hard to create others):

- `AddSqlServerLockAndRunMethods(connectionString)`, which works with a SQL Server database
- `AddPostgreSqlLockAndRunMethods(connectionString)`, which is for a PostGreSQL database
- `AddFileSystemLockAndRunMethods(- path to global directory -)`, which uses a FileSystem directory shared across all of the application's instances.

_NOTE: It is fairly easy to add other LockAndRun methods as long as the [DistributedLock](https://github.com/madelson/DistributedLock) has a lock version for a global lock available to your application._

The reason for having a series of LockAndRun methods is to allow for resources that might not created yet. For instance, if the database doesn't currently exist, then it can't obtain a lock on the database. In this case the second FileSystem LockAndRun method can obtain a lock on a FileSystem directory that all the applications can access. 

The first LockAndRun method that obtains a global lock will then run all your startup services you have registered to RunMethodsSequentially (see `RegisterServiceToRunInJob<T>` section later). Once the your startup services have run on one application instance it is on, then it releases the global lock, which allows another instance of the application to run until all the instances have been run.

Of course, once one instance has successfully applied your startup services to the global resource, then other instances are still going to run. That is why your startup services must check if the update they are planning to add haven't already been applied.

#### Extra options

The `options` also has default settings for some of the code, but you can override these. They are:

- `options.RegisterAsHostedService`: By default this is true, and the the `IGetLockAndThenRunServices` service isn't registered, but the [GetLockAndThenRunHostedService](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/LockAndRunCode/GetLockAndThenRunHostedService.cs) is registered as a `HostedService`. In ASP.NET Core that means the `IGetLockAndThenRunServices` code is run on startup.
- `options.DefaultLockTimeoutInSeconds`: By default this is set to 100 seconds, and defines the time it will wait for a lock can be acquired. _NOTE: When you have `NNN` multiple instances the time for the ALL of your startup services must be less that `DefaultLockTimeoutInSeconds / NNN`.

## `RegisterServiceToRunInJob<T>` extension method

The `RegisterServiceToRunInJob<T>` extension method is used to register your _startup services_ that you want to run on startup. Examples of startup services you might like to write are:

- Migrate/Create your database
- Migrate/Create your individual user account database
- Ensure that a admin user is added to your individual user account database
- Seed your own database

There are two rules about your startup services:

1. They must inherit the [`IServiceToCallWhileInLock`](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/IServiceToCallWhileInLock.cs) interface.
2. You must register your startup services in the correct order of running, e.g. you should add your Migrate/Create your database startup service before any startup services that access that database.

### An example startup service

Here is an example of a startup service that will migrate a database using Entity Framework Core, with a `DbContext` called `TestDbContext`.

```c#
public class MigrateDbContextService : IServiceToCallWhileInLock
{
    private readonly TestDbContext _context;

    public MigrateDbContextService(TestDbContext context)
    {
        _context = context;
    }

    public async ValueTask RunMethodWhileInLockAsync()
    {
        //NOTE: The Migrate method will only update 
        //the database if there any new migrations to add
        await _context.Database.MigrateAsync();
    }
}
```

Because your startup services are register with the .NET dependency injection provider you can inject any other services into your startup service, in the example above it obtains the application's DbContext.
