using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.MessageBroker.Common;

internal static class AmazonSQSCommons
{
    internal static async Task<string> DeclareQueueAsync(Func<IAmazonSQS> sqsClient, ConcurrentDictionary<string, string> queueUrls,
        ILogger logger, MessageBrokerProviderType messageBrokerProvider,
        string queue, IDictionary<string, string>? attributes = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        var request = new CreateQueueRequest
        {
            QueueName = queue
        };

        if (attributes != null)
        {
            request.Attributes = new Dictionary<string, string>(attributes);
        }

        try
        {
            var response = await sqsClient().CreateQueueAsync(request, cancellationToken).ConfigureAwait(false);

            queueUrls[queue] = response.QueueUrl;

            MessageBrokerLog.Declared(logger, messageBrokerProvider, "queue", queue);

            return response.QueueUrl;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeclareFailed(logger, exception, messageBrokerProvider, "queue", queue);

            throw;
        }
    }

    internal static async Task DeleteQueueAsync(Func<IAmazonSQS> sqsClient, ConcurrentDictionary<string, string> queueUrls,
        ILogger logger, MessageBrokerProviderType messageBrokerProvider,
        string queue, bool ignoreIfDoesNotExist = true, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queue, nameof(queue));

        try
        {
            var response = await sqsClient().GetQueueUrlAsync(queue, cancellationToken).ConfigureAwait(false);

            _ = await sqsClient().DeleteQueueAsync(response.QueueUrl, cancellationToken).ConfigureAwait(false);

            MessageBrokerLog.Deleted(logger, messageBrokerProvider, "queue", queue);
        }
        catch (QueueDoesNotExistException) when (ignoreIfDoesNotExist)
        {
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            MessageBrokerLog.DeleteFailed(logger, exception, messageBrokerProvider, "queue", queue);

            throw;
        }
        finally
        {
            _ = queueUrls.TryRemove(queue, out _);
        }
    }

    internal static async Task<bool> HasRedrivePolicyAsync(Func<IAmazonSQS> sqsClient, string queueUrl, CancellationToken cancellationToken)
    {
        var response = await sqsClient().GetQueueAttributesAsync(queueUrl, ["RedrivePolicy"], cancellationToken).ConfigureAwait(false);

        return !string.IsNullOrWhiteSpace(response.Attributes.GetValueOrDefault("RedrivePolicy"));
    }
}
