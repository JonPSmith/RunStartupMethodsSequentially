# Release Notes

## 1.3.0

- Added logging to the locking / running startup service as an exception in a HostedService is hard to track in production

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