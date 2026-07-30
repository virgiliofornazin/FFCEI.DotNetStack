using Amazon.SQS;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.AmazonSQSwithSNS;

public sealed class AmazonSQSwithSNSReceiver(string subject, WeakReference<AmazonSQSwithSNSConnector> connector)
    : AmazonSQSReceiverBase<AmazonSQSwithSNSConnector>(MessageBrokerProviderType.AmazonSQSwithSNS, subject, connector)
{
    protected override void ValidateAddress(MessageBrokerAddress address)
        => _ = AmazonSQSwithSNSConnector.GetReceiverQueueName(address, Subject);

    protected override IAmazonSQS SqsClientOf(AmazonSQSwithSNSConnector connector)
        => connector.SqsClient;

    protected override Task<string> ResolveQueueUrlAsync(AmazonSQSwithSNSConnector connector, MessageBrokerAddress address, CancellationToken cancellationToken)
        => connector.GetQueueUrlAsync(AmazonSQSwithSNSConnector.GetReceiverQueueName(address, Subject), cancellationToken);

    protected override Task<bool> HasRedrivePolicyAsync(AmazonSQSwithSNSConnector connector, string queueUrl, CancellationToken cancellationToken)
        => connector.HasRedrivePolicyAsync(queueUrl, cancellationToken);
}
