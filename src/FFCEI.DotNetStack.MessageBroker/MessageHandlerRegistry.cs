using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.MessageBroker;

public class MessageHandlerRegistry
    : MessageCounter, IMessageHandlerRegistry
{
    private readonly ConcurrentDictionary<Type, IMessageEnvelopeHandler> _handlers = new();

    public static readonly MessageHandlerRegistry Global = new();

    public Action<IMessageEnvelope>? OnUnknownMessageReceived { get; set; }
    public Action<IMessageEnvelope>? OnUnhandledMessage { get; set; }
    public Func<Exception, IMessageEnvelope?, bool>? OnMessageHandlingException { get; set; }

    public void AddMessageHandler(IMessageEnvelopeHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        if (!TryAddMessageHandler(handler))
        {
            throw new InvalidOperationException($"Handler for message type {handler.MessageType} (handler type {handler.GetType()}) already registered");
        }
    }

    public void AddMessageHandler<THandler>() where THandler : IMessageEnvelopeHandler
    {
        if (!TryAddMessageHandler<THandler>())
        {
            throw new InvalidOperationException($"Handler type {typeof(THandler)} already registered");
        }
    }

    public bool TryAddMessageHandler(IMessageEnvelopeHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));

        var result = _handlers.TryAdd(handler.MessageType, handler);

        if (!result)
        {
            handler.DisposeObjectSafelyIfNeeded();
        }

        return result;
    }

    public bool TryAddMessageHandler<THandler>() where THandler : IMessageEnvelopeHandler
    {
        var handler = Activator.CreateInstance<THandler>();

        return TryAddMessageHandler(handler);
    }

    public bool CanHandleMessageOFType(Type messageType)
        => _handlers.ContainsKey(messageType);

    public Type? FindMessageType(string payloadTypeName)
    {
        foreach (var messageType in _handlers.Keys)
        {
            if (string.Equals($"{messageType.Namespace}.{messageType.Name}", payloadTypeName, StringComparison.Ordinal))
            {
                return messageType;
            }
        }

        return null;
    }

    protected async ValueTask OnReceivedAsync(IMessageEnvelope messageEnvelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messageEnvelope, nameof(messageEnvelope));

        using var activity = MessageBrokerTracing.RestoreActivity(messageEnvelope.Attributes,
            messageEnvelope.Attributes.PayloadType ?? "MessageBroker.Receive");

        if (messageEnvelope.Payload == null)
        {
            InvokeUnknownMessageReceived(messageEnvelope);

            return;
        }

        var messageType = (messageEnvelope as MessageEnvelope)?.PayloadType ?? messageEnvelope.Payload.GetType();

        if (_handlers.TryGetValue(messageType, out var handler))
        {
            await InvokeHandlerAsync(handler, messageType, messageEnvelope, cancellationToken).ConfigureAwait(false);
        }
        else if (Global._handlers.TryGetValue(messageType, out var handlerGlobal))
        {
            await InvokeHandlerAsync(handlerGlobal, messageType, messageEnvelope, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await InvokeUnhandledMessageAsync(messageEnvelope).ConfigureAwait(false);
        }
    }

    public IMessageHandlerCallback ConfigureCallbacks(Action<IMessageHandlerCallback> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        expression(this);

        return this;
    }

    private async Task InvokeHandlerAsync(IMessageEnvelopeHandler messageHandler, Type messageType, IMessageEnvelope messageEnvelope, CancellationToken cancellationToken)
    {
        try
        {
            await messageHandler.InvokeHandlerGenericAsync(messageType, messageEnvelope, cancellationToken).ConfigureAwait(false);

            IncrementMessageCount();
        }
        catch (Exception exception)
        {
            IncrementErrorCount();

            if (OnMessageHandlingException != null)
            {
                var result = ExceptionProtector.ExecuteFunctionAndReturnDefaultValue(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));

                if (!(result ?? false))
                {
                    throw new MessageHandlingAlreadyReportedException(exception);
                }
            }
            else
            {
                throw;
            }
        }
    }

    protected async ValueTask InvokeUnhandledMessageAsync(IMessageEnvelope messageEnvelope)
    {
        try
        {
            OnUnhandledMessage?.Invoke(messageEnvelope);

            IncrementMessageCount();
        }
        catch (Exception exception)
        {
            IncrementErrorCount();

            if (OnMessageHandlingException != null)
            {
                var result = ExceptionProtector.ExecuteFunctionAndReturnDefaultValue(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));

                if (!(result ?? false))
                {
                    throw;
                }
            }
            else
            {
                throw;
            }
        }
    }

    protected void InvokeUnknownMessageReceived(IMessageEnvelope messageEnvelope)
    {
        try
        {
            OnUnknownMessageReceived?.Invoke(messageEnvelope);

            IncrementMessageCount();
        }
        catch (Exception exception)
        {
            IncrementErrorCount();

            if (OnMessageHandlingException != null)
            {
                var result = ExceptionProtector.ExecuteFunctionAndReturnDefaultValue(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));

                if (!(result ?? false))
                {
                    throw;
                }
            }
            else
            {
                throw;
            }
        }
    }
}
