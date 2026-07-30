namespace FFCEI.DotNetStack.MessageBroker.Common;

internal static class Constants
{
    internal const string ContentTypeHeaderKey = "Content-Type";
    internal const string ContentEncodingHeaderKey = "Content-Encoding";

    internal const string ContentTypeValue = "application/x-msgpack";
    internal const string ContentEncodingValue = "binary";

    internal const string TraceParentHeaderKey = "traceparent";
    internal const string TraceStateHeaderKey = "tracestate";
    internal const string BaggageHeaderKey = "baggage";

    internal const string MessageIdHeaderKey = "X-FFCEI-MessageId";
    internal const string SendTimestampHeaderKey = "X-FFCEI-SendTimestamp";
    internal const string PayloadTypeHeaderKey = "X-FFCEI-PayloadType";
    internal const string CorrelationIdHeaderKey = "X-FFCEI-CorrelationId";
    internal const string TTLHeaderKey = "X-FFCEI-TTL";

    internal static readonly HashSet<string> KnownHeaderKeys = new()
        {
            ContentTypeHeaderKey,
            ContentEncodingHeaderKey,
            TraceParentHeaderKey,
            TraceStateHeaderKey ,
            BaggageHeaderKey ,
            MessageIdHeaderKey ,
            SendTimestampHeaderKey ,
            PayloadTypeHeaderKey ,
            CorrelationIdHeaderKey ,
            TTLHeaderKey
        };
}
