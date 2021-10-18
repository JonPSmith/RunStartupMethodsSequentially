# RunStartupMethodsSequentially

This .NET library is designed to handle updates to single resources, e.g. a Database, on startup of an application that has multiple instances running (e.g. a ASP.NET Core web site with _Azure Scale Out_ is turned on). This library runs on when the application's startup and apples a migration to a database, seed a database and/or ensuring that an admin user has been added to the authentication database. It can be used with non-database resources too, e.g. blob, files, etc.

> **Scale Out**: A scale out operation is the equivalent of creating multiple copies of your web site and adding a load balancer to distribute the demand  between them (taken from [Microsoft Azure docs](https://azure.microsoft.com/en-gb/blog/scaling-up-and-scaling-out-in-windows-azure-web-sites/)).

This open-source library available on NuGet (!!! not yet). The documentation can be found in this README file and see the [Release notes](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/ReleaseNotes.md) for details of changes.

## How it works, and what to watch out for

This library allows you create services known as _startup services_ which are run sequentially on startup within a [DistributedLock](https://github.com/madelson/DistributedLock) global lock that covers all the application's instances. This stops the problem of multiple startup services clashing, which can cause problems (EF Core's Migrate feature doesn't work if multiple migrations are applied at the same time).

But be aware every startup service will be run on every application's instances, which means your startup services should check if the database has already been updated, e.g. if your service adds an admin user to the the authentication database it should first check that that admin user isn't already been added.

Another warning is that applying a database migration when  

## What you have to do to use this library?

1. Add the RunStartupMethodsSequentially NuGet package (!!! not out yet)to your main application, typically an ASP.NET Core project
2. Create some _startup services_ that will update a common resource, e.g. Migrating a database. Each startup service must implement the RunMethodsSequentially's [IServiceToCallWhileInLock](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/IServiceToCallWhileInLock.cs).
3. Register the RunMethodsSequentially library to your dependency injection provider, and select:
   - What global resource(s) you want to lock on.
   - Register your startup services.

If you are working with ASP.NET Core, then that is all you need to do. If you are working with another approach, like a [Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host) you have to manually the `IGetLockAndThenRunServices` service.

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

This allows you to register one or more _LockAndRun_ methods. Each LockAndRun method first checks that the provided resource type exists. If the resource isn't found, then it exits and allows later LockAndRun methods to try to get a lock on a resource.

_NOTE: This approach is used to handle the situation where the database doesn't currently exist. So, if the lock based on the database fails because there is no database, then the second LockAndRun method can use another approach to gain a lock, e.g. using a FileSystem lock on a Directory._

The first LockAndRun method that found its resource will obtain a global lock and the run all your startup services you have registered with RunMethodsSequentially.

Once the your startup services have run on one instance it will then releases the global lock. That allows another instance of the application to run, until all the instances have run.

#### Extra options

The `options` also has default settings for some of the code, but you can override these. They are:

- `options.RegisterAsHostedService`: By default this is true, and the the `IGetLockAndThenRunServices` service isn't registered, but the [GetLockAndThenRunHostedService](https://github.com/JonPSmith/RunStartupMethodsSequentially/blob/main/RunMethodsSequentially/LockAndRunCode/GetLockAndThenRunHostedService.cs) is registered as a `HostedService`. In ASP.NET Core that means the `IGetLockAndThenRunServices` code is run on startup.
- `options.DefaultLockTimeoutInSeconds`: By default this is set to 100 seconds, and defines the time it will wait for a lock can be acquired. _NOTE: When you have `NNN` multiple instances the time for the ALL of your startup services must be less that `DefaultLockTimeoutInSeconds / NNN`.
- `options.GlobalLockName`: This defines the name used for the lock. At this point its not worth changing but future features might 
