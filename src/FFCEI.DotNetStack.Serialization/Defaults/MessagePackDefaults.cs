using FFCEI.DotNetStack.Serialization.Formatters.MessagePack;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;

namespace FFCEI.DotNetStack.Serialization.Defaults;

public static class MessagePackDefaults
{
    private static readonly IMessagePackFormatter[] _baseFormatters =
        [
            NativeDBNullMessagePackFormatter.Instance,
            NativeDateTimeArrayFormatter.Instance,
            NativeDateTimeFormatter.Instance,
            NativeDecimalFormatter.Instance,
            NativeGuidFormatter.Instance
        ];

    private static readonly IMessagePackFormatter[] _trustedFormatters =
        [.. _baseFormatters, TypelessFormatter.Instance];

    private static readonly IMessagePackFormatter[] _untrustedFormatters =
        [.. _baseFormatters];

    private static readonly IFormatterResolver[] _trustedResolvers =
        [
            NativeDateTimeResolver.Instance,
            NativeDecimalResolver.Instance,
            NativeGuidResolver.Instance,
            StandardResolverAllowPrivate.Instance,
            ContractlessStandardResolver.Instance,
            TypelessContractlessStandardResolver.Instance,
            DynamicGenericResolver.Instance
        ];

    private static readonly IFormatterResolver[] _untrustedResolvers =
        [
            NativeDateTimeResolver.Instance,
            NativeDecimalResolver.Instance,
            NativeGuidResolver.Instance,
            StandardResolverAllowPrivate.Instance,
            ContractlessStandardResolver.Instance,
            DynamicGenericResolver.Instance
        ];

    private static readonly MessagePackSerializerOptions _baseSerializationOptions =
        MessagePackSerializerOptions.Standard.WithOmitAssemblyVersion(true);

    public static IFormatterResolver TrustedCompositeResolver => field ??=
        CompositeResolver.Create(_trustedFormatters, _trustedResolvers);

    public static IFormatterResolver UntrustedCompositeResolver => field ??=
        CompositeResolver.Create(_untrustedFormatters, _untrustedResolvers);

    public static MessagePackSerializerOptions TrustedUncompressedSerializationOptions => field ??=
        _baseSerializationOptions.WithResolver(TrustedCompositeResolver).WithSecurity(MessagePackSecurity.TrustedData);

    public static MessagePackSerializerOptions TrustedCompressedSerializationOptions => field ??=
        TrustedUncompressedSerializationOptions.WithCompression(MessagePackCompression.Lz4BlockArray);

    public static MessagePackSerializerOptions UntrustedUncompressedSerializationOptions => field ??=
        _baseSerializationOptions.WithResolver(UntrustedCompositeResolver).WithSecurity(MessagePackSecurity.UntrustedData);

    public static MessagePackSerializerOptions UntrustedCompressedSerializationOptions => field ??=
        UntrustedUncompressedSerializationOptions.WithCompression(MessagePackCompression.Lz4BlockArray);
}
