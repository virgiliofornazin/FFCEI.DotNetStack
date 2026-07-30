using FFCEI.DotNetStack.Serialization;
using System.Collections.ObjectModel;

namespace FFCEI.DotNetStack.MessageBroker.Configurations;

public sealed class MessageBrokerConnectorFactoryConfiguration
{
    public SerializationAlgorithm? SerializationAlgorithm { get; set; }
    public required ReadOnlyCollection<MessageBrokerConnectionConfiguration> Connections { get; set; }
    public ReadOnlyCollection<MessageBrokerSubjectConfiguration>? Subjects { get; set; }
    public ReadOnlyCollection<MessageBrokerAutomaticReceiverConfiguration>? AutomaticReceivers { get; set; }

    public MessageBrokerConnectionConfiguration? DefaultConnection => Connections == null ? null :
        Connections.FirstOrDefault(connection => connection.IsDefault) ??
        Connections.FirstOrDefault();

    public MessageBrokerSubjectConfiguration? DefaultSubject => Subjects == null ? null :
        Subjects.FirstOrDefault(connection => connection.IsDefault) ??
        Subjects.FirstOrDefault();

    public bool HasSubjects => (Subjects != null && Subjects.Count > 0);

    public bool HasAutomaticReceivers => (AutomaticReceivers != null && AutomaticReceivers.Count > 0);

    internal void ValidateConfiguration()
    {
        if ((Connections == null) || (Connections.Count == 0))
        {
            throw new MessageBrokerConnectorFactoryConfigurationException($"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: no connections are defined");
        }

        var hasManyConnections = false;
        var hasManySubjects = false;

        var connectionIds = Connections.Where(connection => !string.IsNullOrEmpty(connection.ConnectionId)).Select(connection => connection.ConnectionId).ToHashSet() ?? [];
        var subjects = new HashSet<string>();

        if (Connections.Count > 1)
        {
            if (connectionIds.Count < Connections.Count)
            {
                throw new MessageBrokerConnectorFactoryConfigurationException(
                    $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: each connection must have a unique connection identification defined");
            }

            hasManyConnections = true;
        }

        if (Connections != null && Connections.Any(connection => connection.QueueDeclarations != null && connection.QueueDeclarations.Any(queue => string.IsNullOrWhiteSpace(queue.QueueName) ||
            (queue.Bindings != null && queue.Bindings.Any(binding => !binding.BindingAddress.IsValidAddress)))))
        {
            throw new MessageBrokerConnectorFactoryConfigurationException(
                $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: queue declarations with invalid address bindings");
        }

        if (Connections != null && Connections.Any(connection => connection.ExchangeDeclarations != null && connection.ExchangeDeclarations.Any(exchange =>
            string.IsNullOrWhiteSpace(exchange.ExchangeName) || exchange.ExchangeType is not (MessageBrokerAddressType.Direct or MessageBrokerAddressType.Topic or
                MessageBrokerAddressType.Fanout or MessageBrokerAddressType.Headers))))
        {
            throw new MessageBrokerConnectorFactoryConfigurationException(
                $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: exchange declarations with invalid name or unsupported exchange type");
        }

        var defaultConnectionId = DefaultConnection?.ConnectionId;
        var defaultSubject = string.Empty;

        if ((Subjects != null) && (Subjects.Count > 0))
        {
            subjects = [.. Subjects.Select(subject => subject.Name)];

            if (Subjects.Any(subject => string.IsNullOrWhiteSpace(subject.Name)))
            {
                throw new MessageBrokerConnectorFactoryConfigurationException(
                    $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: each subject must have a name defined");
            }

            if (subjects.Count < Subjects.Count)
            {
                throw new MessageBrokerConnectorFactoryConfigurationException(
                    $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: each subject must have a unique name");
            }

            var validSubjects = hasManyConnections ?
                Subjects.All(subject => !string.IsNullOrWhiteSpace(subject.ConnectionId) && connectionIds.Contains(subject.ConnectionId)) :
                Subjects.All(subject => string.IsNullOrWhiteSpace(subject.ConnectionId) || (subject.ConnectionId == defaultConnectionId));

            if (!validSubjects)
            {
                throw new MessageBrokerConnectorFactoryConfigurationException(
                    $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: each subject key must have a valid connection identification assigned or empty if only one connection is defined");
            }

            hasManySubjects = Subjects.Count > 1;
            defaultSubject = DefaultSubject?.Name;
        }

        if ((AutomaticReceivers != null) && (AutomaticReceivers.Count > 0))
        {
            if (AutomaticReceivers.Any(automaticReceiver => !automaticReceiver.Address.IsValidAddress))
            {
                throw new MessageBrokerConnectorFactoryConfigurationException(
                    $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: invalid address configured for automatic receiver");
            }

            if (HasSubjects)
            {
                var validAutomaticReceivers = hasManySubjects ?
                    AutomaticReceivers.All(automaticReceiver => !string.IsNullOrWhiteSpace(automaticReceiver.Subject) && subjects.Contains(automaticReceiver.Subject)) :
                    AutomaticReceivers.All(automaticReceiver => string.IsNullOrWhiteSpace(automaticReceiver.Subject) || (automaticReceiver.Subject == defaultSubject));

                if (!validAutomaticReceivers)
                {
                    throw new MessageBrokerConnectorFactoryConfigurationException(
                        $"{MessageBrokerConnectorFactoryConfigurationException.DefaultMessage}: each automatic receiver must have a valid subject name assigned or empty if only one (or none) subjects are defined");
                }
            }
        }
    }
}
