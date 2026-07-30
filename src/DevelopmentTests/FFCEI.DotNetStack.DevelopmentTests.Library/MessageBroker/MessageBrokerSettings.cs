using FFCEI.DotNetStack.DevelopmentTests.Library.MessageHandlers;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker;
using FFCEI.DotNetStack.MessageBroker.Configurations;
using FFCEI.DotNetStack.Serialization;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.MessageBroker;

public static class MessageBrokerSettings
{
    public static MessageBrokerConnectorFactoryConfiguration Default => RabbitMQ;

    public static readonly MessageBrokerAddress TestTargetAddress = MessageBrokerAddress.Queue("TestTarget");

    public static readonly MessageBrokerConnectorFactoryConfiguration Channels = new()
    {
        Connections =
            [
                new()
                {
                    ProviderType = MessageBrokerProviderType.Channels,
                    ConnectionString = string.Empty
                }
            ],
        Subjects =
            [
                new()
                {
                    Name = "Default",
                    IsDefault = true
                }
            ],
        AutomaticReceivers =
            [
                new()
                {
                    Address = TestTargetAddress,
                    Handlers =
                    [
                        new()
                        {
                            HandlerTypename = typeof(BlogRowMessageHandler).NamespaceAndName()
                        }
                    ]
                }
            ],
        SerializationAlgorithm = SerializationAlgorithm.Unspecified
    };

    public static readonly MessageBrokerConnectorFactoryConfiguration Qpid = new()
    {
        Connections =
            [
                new()
                {
                    ProviderType = MessageBrokerProviderType.Qpid,
                    ConnectionString = "amqp://10.1.129.2:55672",
                    QueueDeclarations =
                        [
                            new()
                            {
                                QueueName = "TestTarget",
                                Bindings =
                                    [
                                        new()
                                        {
                                            BindingAddress = MessageBrokerAddress.Topic("TestTarget.*")
                                        }
                                    ]
                            }
                        ]
                }
            ],
        Subjects =
            [
                new()
                {
                    Name = "Default",
                    IsDefault = true
                }
            ],
        AutomaticReceivers =
            [
                new()
                {
                    Address = TestTargetAddress,
                    Handlers =
                    [
                        new()
                        {
                            HandlerTypename = typeof(BlogRowMessageHandler).NamespaceAndName()
                        }
                    ]
                }
            ],
        SerializationAlgorithm = SerializationAlgorithm.Unspecified
    };

    public static readonly MessageBrokerConnectorFactoryConfiguration RabbitMQ = new()
    {
        Connections =
            [
                new()
                {
                    ProviderType = MessageBrokerProviderType.RabbitMQ,
                    ConnectionString = "amqp://dotnetstack:dotnetstack@10.1.129.2:5672",
                    QueueDeclarations =
                        [
                            new()
                            {
                                QueueName = "TestTarget",
                                Bindings =
                                    [
                                        new()
                                        {
                                            BindingAddress = MessageBrokerAddress.Topic("TestTarget.*")
                                        }
                                    ]
                            }
                        ]
                }
            ],
        Subjects =
            [
                new()
                {
                    Name = "Default",
                    IsDefault = true
                }
            ],
        AutomaticReceivers =
            [
                new()
                {
                    Address = TestTargetAddress,
                    Handlers =
                    [
                        new()
                        {
                            HandlerTypename = typeof(BlogRowMessageHandler).NamespaceAndName()
                        }
                    ]
                }
            ],
        SerializationAlgorithm = SerializationAlgorithm.Unspecified
    };

    public static readonly MessageBrokerConnectorFactoryConfiguration Kafka = new()
    {
        Connections =
            [
                new()
                {
                    ProviderType = MessageBrokerProviderType.Kafka,
                    ConnectionString = "{ \"bootstrap.servers\": \"10.1.129.2:9092\", \"socket.keepalive.enable\": \"true\" }",
                    QueueDeclarations =
                        [
                            new()
                            {
                                QueueName = "TestTarget"
                            }
                        ]
                }
            ],
        Subjects =
            [
                new()
                {
                    Name = "Default",
                    IsDefault = true
                }
            ],
        AutomaticReceivers =
            [
                new()
                {
                    Address = TestTargetAddress,
                    Handlers =
                    [
                        new()
                        {
                            HandlerTypename = typeof(BlogRowMessageHandler).NamespaceAndName()
                        }
                    ]
                }
            ],
        SerializationAlgorithm = SerializationAlgorithm.Unspecified
    };
}
