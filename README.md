FFCEI.DotNetStack

A collection of self-develop libraries to create Web API Services in .NET (coded in C#) with common services design patterns

- ORM Model Library with common patterns (Unique ID, Externally accessed with UUID, Timestamp Tracked, Logical Delete support);
- Externed ORM support for EntityFramework Core (with DLL specializations for most common databases: SQL Server, Oracle, MySQL, PostgreSQL, MySQL), with auto mapping, type detection, support for logically deleted objects (flags, query filtering, etc);
- Distributed Database Caching (2 layers, local RAM and Redis Service using FusionCache);
- Secrets support (supporting AWS, Azure, Docker, GCP, OCI secrets support)
- Serialization support for inter-service messaging using Json / MessagePack specializations;
- MessageBroker support (.NET Channels, AmazonSQS/SNS, Azure Service Bus, Google PubSub, Kakfa, Qpid/Proton AMQP 1.0), RabbitMQ)
- Configuration support (ENV file loading)
- Compliance Redacting Library (LGPD / GPDR / custom redacting)
- AccessControl / WebApi (they are work in progress) -> support for authentication, authorization, session management, access control lists and JWT token authorization.
