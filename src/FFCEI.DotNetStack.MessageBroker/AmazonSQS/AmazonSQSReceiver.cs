using Amazon.SQS;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.AmazonSQS;

public sealed class AmazonSQSReceiver(string subject, WeakReference<AmazonSQSConnector> connector)
    : AmazonSQSReceiverBase<AmazonSQSConnector>(MessageBrokerProviderType.AmazonSQS, subject, connector)
{
    protected override void ValidateAddress(MessageBrokerAddress address)
        => _ = AmazonSQSConnector.ExtractQueueName(address);

    protected override IAmazonSQS SqsClientOf(AmazonSQSConnector connector)
        => connector.Client;

    protected override Task<string> ResolveQueueUrlAsync(AmazonSQSConnector connector, MessageBrokerAddress address, CancellationToken cancellationToken)
        => connector.GetQueueUrlAsync(address, cancellationToken);

    protected override Task<bool> HasRedrivePolicyAsync(AmazonSQSConnector connector, string queueUrl, CancellationToken cancellationToken)
        => connector.HasRedrivePolicyAsync(queueUrl, cancellationToken);
}
