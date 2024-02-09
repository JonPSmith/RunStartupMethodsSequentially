# Release Notes

## 2.0.1

- Microsoft.Data.SqlClient updated to because of vulnerable in current version
- Updated DistributedLock parts to fix the .NET 8 Postgres bug - see https://github.com/npgsql/npgsql/issues/5143

## 2.0.0

- Changed framework to netstandard2.1 to work with any version of .NET

## 1.3.1 

- Update of various NuGet packages, especially the DistributedLock packages that fixes some problems 

## 1.3.0

- New Feature: Added a class to test that your startup code will run properly
- New Feature: Added logging so that you can see what RunMethodsSequentially did on host startup

## 1.2.0

- BREAKING CHANGE: `RunMethodWhileInLockAsync()` in old interface `IServiceToCallWhileInLock` changed to `ApplyYourChangeAsync(IServiceProvider scopedServices)`. This provides a another way to obtain services instead of constructor injection 
- BREAKING CHANGE: `OrderNum` added to old interface `IServiceToCallWhileInLock` to define the order your startup services are run
- BREAKING CHANGE: interface `IServiceToCallWhileInLock` renamed to `IStartupServiceToRunSequentially`
- BREAKING CHANGE: Deleted `WhatOrderToRunIn` attribute as `OrderNum` now added to `IStartupServiceToRunSequentially` interface
- New feature: Added `AddRunMethodsWithoutLock` version as a way of turning off locking if not needed

## 1.1.0

- Added the optional WhatOrderToRunIn attribute to allow you to define the order in which the IServiceToCallWhileInLock services are run

## 1.0.0

- First release: Supports SQL Server and PostgreSQL (other databases available)