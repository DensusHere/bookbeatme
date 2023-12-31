﻿# KnightBus.Azure.ServiceBus Changelog


## 17.0.0
### Changed 
- Default is now shared connection of service bus client

## 16.0.0
### Added/Changed 
- Shared connection support of service bus client

## 15.1.2
### Changed 
- Simplify restart handling when using `IRestartTransportOnIdle`.

## 15.1.1
### Fixed
- Prevent potential memory leak when using `IRestartTransportOnIdle`.

## 15.1.0
### Added
- Possibility to automatically force restart of Azure ServiceBus receivers after a fixed idle period (i.e. no messages processed). Specified through `IRestartTransportOnIdle`.

## 9.0.0

- `ServiceBusCreationOptions` implements `IServiceBusCreationOptions`.
- To tell the Azure ServiceBus queue/topic to override default creation options, add IServiceBusCreationOptions to IMessageMapping implementation.

Example:

```
    public class MyMessage : IServiceBusCommand
    {
        public string Message { get; set; }
    }

    public class MyMessageMapping : IMessageMapping<MyMessage>, IServiceBusCreationOptions
    {
        public string QueueName => "your-queue";
		
        public bool EnablePartitioning => true;
        public bool SupportOrdering => false;
        public bool EnableBatchedOperations => true;
    }
```