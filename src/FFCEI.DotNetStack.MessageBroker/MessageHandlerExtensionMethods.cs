using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.MessageBroker.Interfaces.Generic;
using System.Collections.Concurrent;
using System.Reflection;

namespace FFCEI.DotNetStack.MessageBroker;

public static class MessageHandlerExtensionMethods
{
    internal static Task InvokeHandlerAsync<TMessage>(this IMessageEnvelopeHandler messageHandler, IMessageEnvelope messageEnvelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messageHandler, nameof(messageHandler));

        if (messageHandler is not IMessageHandler<TMessage> handler)
        {
            throw new InvalidCastException($"Cannot cast message handler of type {messageHandler.GetType()} to type of message handler of type {typeof(TMessage)}");
        }

        return handler.HandleAsync(messageEnvelope, cancellationToken);
    }

    private static readonly ConcurrentDictionary<Type, MethodInfo> _invokeHandlerGenericAsyncMethodCache = new();

    private static readonly MethodInfo? _methodInvokeHandlerAsync = typeof(MessageHandlerExtensionMethods)
        .GetMethod(nameof(InvokeHandlerAsync), BindingFlags.NonPublic | BindingFlags.Static);

    internal static Task InvokeHandlerGenericAsync(this IMessageEnvelopeHandler messageHandler, Type messageType, IMessageEnvelope messageEnvelope, CancellationToken cancellationToken)
    {
        var invokeHandlerGenericAsyncMethod = _invokeHandlerGenericAsyncMethodCache.GetOrAdd(messageType, (_) =>
        {
            ArgumentNullException.ThrowIfNull(_methodInvokeHandlerAsync, nameof(_methodInvokeHandlerAsync));

            return _methodInvokeHandlerAsync.MakeGenericMethod(messageType);
        });

        var task = invokeHandlerGenericAsyncMethod.Invoke(null, [messageHandler, messageEnvelope, cancellationToken]);

        if (task is not Task invokeTask)
        {
            throw new InvalidOperationException($"Failed to invoke generic handler for message type {messageType.Name}");
        }

        return invokeTask;
    }
}
