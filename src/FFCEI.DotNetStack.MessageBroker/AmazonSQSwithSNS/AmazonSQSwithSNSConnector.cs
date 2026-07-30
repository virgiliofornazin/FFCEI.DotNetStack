using Amazon;
using Amazon.Runtime;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace FFCEI.DotNetStack.MessageBroker.AmazonSQSwithSNS;

public sealed class AmazonSQSwithSNSConnector(ILogger<AmazonSQSwithSNSConnector>? logger = null)
        : MessageBrokerConnector(MessageBrokerProviderType.AmazonSQSwithSNS, logger)
{
    private IAmazonSQS? _sqsClient;
    private IAmazonSimpleNotificationService? _snsClient;
    private readonly ConcurrentDictionary<string, string> _queueUrls = [];
    private readonly ConcurrentDictionary<string, string> _topicArns = [];
    private readonly ConcurrentDictionary<WeakReference<AmazonSQSwithSNSReceiver>, object> _receivers = [];

    internal IAmazonSQS SqsClient => _sqsClient != null ? _sqsClient! :
        throw new InvalidOperationException("AmazonSQSwithSNSConnector not started");

    internal IAmazonSimpleNotificationService SnsClient => _snsClient != null ? _snsClient! :
        throw new InvalidOperationException("AmazonSQSwithSNSConnector not started");

    public override bool Started()
        => !Disposed && (_sqsClient != null) && (_snsClient != null);

    private void ThrowIfAlreadyStarted()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if ((_sqsClient != null) || (_snsClient != null))
        {
            throw new InvalidOperationException("AmazonSQSwithSNSConnector already started");
        }
    }

    private void SetClients(Func<(IAmazonSQS SqsClient, IAmazonSimpleNotificationService SnsClient)> factory)
    {
        try
        {
            (_sqsClient, _snsClient) = factory();
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.ConnectorStartFailed(Logger, exception, MessageBrokerProvider);

            throw;
        }

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);
    }

    public Task StartAsync(IAmazonSQS sqsClient, IAmazonSimpleNotificationService snsClient)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentNullException.ThrowIfNull(sqsClient, nameof(sqsClient));
        ArgumentNullException.ThrowIfNull(snsClient, nameof(snsClient));

        ThrowIfAlreadyStarted();

        _sqsClient = sqsClient;
        _snsClient = snsClient;

        MessageBrokerLog.ConnectorStarted(Logger, MessageBrokerProvider);

        return Task.CompletedTask;
    }

    public Task StartAsync(RegionEndpoint region)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClients(() => (new AmazonSQSClient(region), new AmazonSimpleNotificationServiceClient(region)));

        return Task.CompletedTask;
    }

    public Task StartAsync(string accessKey, string clientSecret, RegionEndpoint region)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClients(() => (new AmazonSQSClient(accessKey, clientSecret, region), new AmazonSimpleNotificationServiceClient(accessKey, clientSecret, region)));

        return Task.CompletedTask;
    }

    public Task StartAsync(string accessKey, string clientSecret, AmazonSQSConfig sqsConfig, AmazonSimpleNotificationServiceConfig snsConfig)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClients(() => (new AmazonSQSClient(accessKey, clientSecret, sqsConfig), new AmazonSimpleNotificationServiceClient(accessKey, clientSecret, snsConfig)));

        return Task.CompletedTask;
    }

    public Task StartAsync(AWSCredentials awsCredentials, RegionEndpoint region)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClients(() => (new AmazonSQSClient(awsCredentials, region), new AmazonSimpleNotificationServiceClient(awsCredentials, region)));

        return Task.CompletedTask;
    }

    public Task StartAsync(AWSCredentials awsCredentials, AmazonSQSConfig sqsConfig, AmazonSimpleNotificationServiceConfig snsConfig)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ThrowIfAlreadyStarted();

        SetClients(() => (new AmazonSQSClient(awsCredentials, sqsConfig), new AmazonSimpleNotificationServiceClient(awsCredentials, snsConfig)));

        return Task.CompletedTask;
    }

    public override Task StartAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(uri, nameof(uri));

        return StartAsync(uri.OriginalString, cancellationToken);
    }

    public override Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        ThrowIfAlreadyStarted();

        var value = connectionString.Trim();

        Dictionary<string, string> settings;

        try
        {
            settings = JsonDeserializer.Default.Deserialize<Dictionary<string, string>>(value)?
                .ToDictionary(result => result.Key.ToUpperInvariant(), result => result.Value) ??
                    throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString));
        }
        catch (JsonException exception)
        {
            throw new ArgumentException("Connection string is not a valid JSON settings object", nameof(connectionString), exception);
        }

        var sqsConfig = new AmazonSQSConfig();
        var snsConfig = new AmazonSimpleNotificationServiceConfig();

        if (settings.TryGetValue("REGION", out var region) && !string.IsNullOrWhiteSpace(region))
        {
            sqsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
            snsConfig.RegionEndpoint = RegionEndpoint.GetBySystemName(region);
        }

        if (settings.TryGetValue("SQSSERVICEURL", out var sqsServiceUrl) && !string.IsNullOrWhiteSpace(sqsServiceUrl))
        {
            sqsConfig.ServiceURL = sqsServiceUrl;
        }

        if (settings.TryGetValue("SNSSERVICEURL", out var snsServiceUrl) && !string.IsNullOrWhiteSpace(snsServiceUrl))
        {
            snsConfig.ServiceURL = snsServiceUrl;
        }

        var hasAccessKey = settings.TryGetValue("ACCESSKEY", out var accessKey) && !string.IsNullOrWhiteSpace(accessKey);
        var hasSecretKey = settings.TryGetValue("SECRETKEY", out var secretKey) && !string.IsNullOrWhiteSpace(secretKey);

        if (hasAccessKey && hasSecretKey)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            SetClients(() => (new AmazonSQSClient(credentials, sqsConfig), new AmazonSimpleNotificationServiceClient(credentials, snsConfig)));
        }
        else
        {
            SetClients(() => (new AmazonSQSClient(sqsConfig), new AmazonSimpleNotificationServiceClient(snsConfig)));
        }

        return Task.CompletedTask;
    }

    public override async Task StopAsync()
    {
        foreach (var weakReceiver in _receivers.Keys)
        {
            if (weakReceiver.TryGetTarget(out var receiver) && (receiver != null))
            {
                await ExceptionProtector.ExecuteTaskAsync(receiver.StopAsync()).ConfigureAwait(false);
            }
        }

        _receivers.Clear();
        _queueUrls.Clear();
        _topicArns.Clear();

        _sqsClient?.DisposeSafelyIfNeeded();
        _sqsClient = null;

        _snsClient?.DisposeSafelyIfNeeded();
        _snsClient = null;
    }

    public override Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = SqsClient;
        _ = SnsClient;

        var sender = new AmazonSQSwithSNSSender(subject, new WeakReference<AmazonSQSwithSNSConnector>(this))
        {
            DefaultAddress = defaultAddress,
            Serializer = serializer
        };

        MessageBrokerLog.SenderCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<ISender>(sender);
    }

    public override Task<IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        _ = SqsClient;
        _ = SnsClient;

        var receiver = new AmazonSQSwithSNSReceiver(subject, new WeakReference<AmazonSQSwithSNSConnector>(this))
        {
            Deserializer = deserializer
        };

        _receivers[new WeakReference<AmazonSQSwithSNSReceiver>(receiver)] = true;

        MessageBrokerLog.ReceiverCreated(Logger, MessageBrokerProvider, subject);

        return Task.FromResult<IReceiver>(receiver);
    }

    internal static bool IsTopicAddress(MessageBrokerAddress address)
        => address.Type is MessageBrokerAddressType.Topic or MessageBrokerAddressType.Fanout;

    internal static string ExtractQueueName(MessageBrokerAddress address)
        => address.Type switch
        {
            MessageBrokerAddressType.Direct => address.Value,
            MessageBrokerAddressType.Queue => address.Value,
            _ => throw new NotSupportedException($"Address type '{address.Type}' is not a queue address")
        };

    internal static string ExtractTopicName(MessageBrokerAddress address)
        => address.Type switch
        {
            MessageBrokerAddressType.Topic => address.Value,
            MessageBrokerAddressType.Fanout => address.Value,
            _ => throw new NotSupportedException($"Address type '{address.Type}' is not a topic address")
        };

    public static string SubscriptionQueueName(string topic, string? subject)
        => $"{topic}-{(string.IsNullOrWhiteSpace(subject) ? "default" : subject)}";

    internal static string GetReceiverQueueName(MessageBrokerAddress address, string subject)
        => IsTopicAddress(address) ?
            SubscriptionQueueName(ExtractTopicName(address), subject) :
            ExtractQueueName(address);

    internal async Task<string> GetQueueUrlAsync(string queueName, CancellationToken cancellationToken)
    {
        if (_queueUrls.TryGetValue(queueName, out var queueUrl))
        {
            return queueUrl;
        }

        var response = await SqsClient.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);

        _queueUrls[queueName] = response.QueueUrl;

        return response.QueueUrl;
    }

    internal Task<bool> HasRedrivePolicyAsync(string queueUrl, CancellationToken cancellationToken)
        => AmazonSQSCommons.HasRedrivePolicyAsync(() => SqsClient, queueUrl, cancellationToken);

    internal async Task<string> GetTopicArnAsync(string topicName, CancellationToken cancellationToken)
    {
        if (_topicArns.TryGetValue(topicName, out var topicArn))
        {
            return topicArn;
        }

        var topic = await SnsClient.FindTopicAsync(topicName).WaitAsync(cancellationToken).ConfigureAwait(false) ??
            throw new InvalidOperationException($"SNS topic '{topicName}' not found; declare it first");

        _topicArns[topicName] = topic.TopicArn;

        return topic.TopicArn;
    }

    public Task<string> DeclareQueueAsync(string queue, IDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
        => AmazonSQSCommons.DeclareQueueAsync(() => SqsClient, _queueUrls, Logger, MessageBrokerProvider, queue, attributes, cancellationToken);

    public Task DeleteQueueAsync(string queue, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
        => AmazonSQSCommons.DeleteQueueAsync(() => SqsClient, _queueUrls, Logger, MessageBrokerProvider, queue, ignoreIfDoesNotExist, cancellationToken);

    public async Task<string> DeclareTopicAsync(string topic, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

        try
        {
            var response = await SnsClient.CreateTopicAsync(topic, cancellationToken).ConfigureAwait(false);

            _topicArns[topic] = response.TopicArn;

            MessageBrokerLog.Declared(Logger, MessageBrokerProvider, "topic", topic);

            return response.TopicArn;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeclareFailed(Logger, exception, MessageBrokerProvider, "topic", topic);

            throw;
        }
    }

    public async Task DeleteTopicAsync(string topic, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));

        try
        {
            var existingTopic = await SnsClient.FindTopicAsync(topic).WaitAsync(cancellationToken).ConfigureAwait(false);

            if (existingTopic == null)
            {
                if (ignoreIfDoesNotExist)
                {
                    return;
                }

                throw new InvalidOperationException($"SNS topic '{topic}' not found");
            }

            _ = await SnsClient.DeleteTopicAsync(existingTopic.TopicArn, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(Logger, MessageBrokerProvider, "topic", topic);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeleteFailed(Logger, exception, MessageBrokerProvider, "topic", topic);

            throw;
        }
        finally
        {
            _ = _topicArns.TryRemove(topic, out _);
        }
    }

    public async Task<string> DeclareSubscriptionAsync(string topic, string queue, IDictionary<string, string>? queueAttributes = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        var topicArn = await DeclareTopicAsync(topic, cancellationToken).ConfigureAwait(false);
        var queueUrl = await DeclareQueueAsync(queue, queueAttributes, cancellationToken).ConfigureAwait(false);

        try
        {
            var queueAttributesResponse = await SqsClient.GetQueueAttributesAsync(queueUrl, ["QueueArn"], cancellationToken).ConfigureAwait(false);
            var queueArn = queueAttributesResponse.Attributes["QueueArn"];

            var policy = $$"""
                {
                  "Version": "2012-10-17",
                  "Statement": [
                    {
                      "Effect": "Allow",
                      "Principal": { "Service": "sns.amazonaws.com" },
                      "Action": "sqs:SendMessage",
                      "Resource": "{{queueArn}}",
                      "Condition": { "ArnEquals": { "aws:SourceArn": "{{topicArn}}" } }
                    }
                  ]
                }
                """;

            _ = await SqsClient.SetQueueAttributesAsync(queueUrl, new Dictionary<string, string>
            {
                ["Policy"] = policy
            }, cancellationToken).ConfigureAwait(false);

            var subscribeRequest = new SubscribeRequest
            {
                TopicArn = topicArn,
                Protocol = "sqs",
                Endpoint = queueArn,
                ReturnSubscriptionArn = true,
                Attributes = new Dictionary<string, string>
                {
                    ["RawMessageDelivery"] = "true"
                }
            };

            var subscribeResponse = await SnsClient.SubscribeAsync(subscribeRequest, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.DeclaredSubscription(Logger, MessageBrokerProvider, queue, topic);

            return subscribeResponse.SubscriptionArn;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeclareSubscriptionFailed(Logger, exception, MessageBrokerProvider, queue, topic);

            throw;
        }
    }

    public async Task DeleteSubscriptionAsync(string topic, string queue, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic, nameof(topic));
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        var existingTopic = await SnsClient.FindTopicAsync(topic).WaitAsync(cancellationToken).ConfigureAwait(false);

        if (existingTopic == null)
        {
            if (ignoreIfDoesNotExist)
            {
                return;
            }

            throw new InvalidOperationException($"SNS topic '{topic}' not found");
        }

        string? queueArn = null;

        try
        {
            var queueUrl = await GetQueueUrlAsync(queue, cancellationToken).ConfigureAwait(false);
            var queueAttributesResponse = await SqsClient.GetQueueAttributesAsync(queueUrl, ["QueueArn"], cancellationToken).ConfigureAwait(false);

            queueArn = queueAttributesResponse.Attributes["QueueArn"];
        }
        catch (QueueDoesNotExistException) when (ignoreIfDoesNotExist)
        {
            return;
        }

        string? nextToken = null;

        try
        {
            do
            {
                var response = await SnsClient.ListSubscriptionsByTopicAsync(existingTopic.TopicArn, nextToken, cancellationToken).ConfigureAwait(false);

                var subscription = response.Subscriptions?.FirstOrDefault(item => item.Endpoint == queueArn);

                if (subscription != null)
                {
                    _ = await SnsClient.UnsubscribeAsync(subscription.SubscriptionArn, cancellationToken).ConfigureAwait(false);

                    MessageBrokerLog.DeletedSubscription(Logger, MessageBrokerProvider, queue, topic);

                    return;
                }

                nextToken = response.NextToken;
            }
            while (!string.IsNullOrEmpty(nextToken));
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeleteSubscriptionFailed(Logger, exception, MessageBrokerProvider, queue, topic);

            throw;
        }

        if (!ignoreIfDoesNotExist)
        {
            throw new InvalidOperationException($"SNS subscription of queue '{queue}' on topic '{topic}' not found");
        }
    }

    internal static Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue> MessageAttributesToSNSAttributes(MessageEnvelope messageEnvelope)
    {
        var sentTimestamp = messageEnvelope.Attributes.TouchSentTimestamp();

        var attributes = new Dictionary<string, Amazon.SimpleNotificationService.Model.MessageAttributeValue>
        {
            [Constants.ContentTypeHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = Constants.ContentTypeValue
            },
            [Constants.ContentEncodingHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = Constants.ContentEncodingValue
            },
            [Constants.MessageIdHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = ValueConversions.GuidToString(messageEnvelope.Attributes.MessageId)
            },
            [Constants.SendTimestampHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = ValueConversions.DateTimeOffsetToString(sentTimestamp)
            }
        };

        if (messageEnvelope.Attributes.PayloadType != null)
        {
            attributes[Constants.PayloadTypeHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.PayloadType
            };
        }

        if (messageEnvelope.Attributes.TTL.HasValue)
        {
            attributes[Constants.TTLHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = ((long)messageEnvelope.Attributes.TTL.Value.TotalMilliseconds).ToString(CultureInfo.InvariantCulture)
            };
        }

        if (messageEnvelope.Attributes.TraceParent != null)
        {
            attributes[Constants.TraceParentHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.TraceParent
            };
        }

        if (messageEnvelope.Attributes.TraceState != null)
        {
            attributes[Constants.TraceStateHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.TraceState
            };
        }

        if (messageEnvelope.Attributes.Baggage != null)
        {
            attributes[Constants.BaggageHeaderKey] = new Amazon.SimpleNotificationService.Model.MessageAttributeValue
            {
                DataType = "String",
                StringValue = messageEnvelope.Attributes.Baggage
            };
        }

        return attributes;
    }
}
