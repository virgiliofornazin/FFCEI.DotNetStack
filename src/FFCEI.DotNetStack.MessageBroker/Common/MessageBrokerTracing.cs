using FFCEI.DotNetStack.MessageBroker.Interfaces;
using System.Diagnostics;

namespace FFCEI.DotNetStack.MessageBroker.Common;

public static class MessageBrokerTracing
{
    public const string ActivitySourceName = "FFCEI.DotNetStack.MessageBroker";

    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    private static readonly DistributedContextPropagator _propagator = DistributedContextPropagator.CreateW3CPropagator();

    internal static void CaptureCurrentActivity(MessageEnvelopeAttributes attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes, nameof(attributes));

        _propagator.Inject(Activity.Current, attributes, InjectSetter);
    }

    internal static Activity? RestoreActivity(IMessageEnvelopeAttributes attributes, string activityName)
    {
        ArgumentNullException.ThrowIfNull(attributes, nameof(attributes));

        _propagator.ExtractTraceIdAndState(attributes, ExtractGetter, out var traceParent, out var traceState);

        var baggage = _propagator.ExtractBaggage(attributes, ExtractGetter);

        var parentContext = default(ActivityContext);

        if (!string.IsNullOrEmpty(traceParent))
        {
            _ = ActivityContext.TryParse(traceParent, traceState, out parentContext);
        }

        var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext);

        if ((activity != null) && (baggage != null))
        {
            foreach (var item in baggage)
            {
                _ = activity.AddBaggage(item.Key, item.Value);
            }
        }

        return activity;
    }

    private static void InjectSetter(object? carrier, string fieldName, string fieldValue)
    {
        if (carrier is not MessageEnvelopeAttributes attributes)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(fieldValue))
        {
            return;
        }

        switch (fieldName)
        {
        case Constants.TraceParentHeaderKey:
            {
                attributes.TraceParent = fieldValue;

                break;
            }
        case Constants.TraceStateHeaderKey:
            {
                attributes.TraceState = fieldValue;

                break;
            }
        case Constants.BaggageHeaderKey:
            {
                attributes.Baggage = fieldValue;

                break;
            }
        }
    }

    private static void ExtractGetter(object? carrier, string fieldName, out string? fieldValue, out IEnumerable<string>? fieldValues)
    {
        fieldValues = null;

        if (carrier is not IMessageEnvelopeAttributes attributes)
        {
            fieldValue = null;

            return;
        }

        fieldValue = fieldName switch
        {
            Constants.TraceParentHeaderKey => attributes.TraceParent,
            Constants.TraceStateHeaderKey => attributes.TraceState,
            Constants.BaggageHeaderKey => attributes.Baggage,
            _ => null
        };
    }
}
