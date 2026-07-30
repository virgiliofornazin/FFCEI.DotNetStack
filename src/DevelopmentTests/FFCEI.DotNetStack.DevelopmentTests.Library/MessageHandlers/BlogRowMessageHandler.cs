using FFCEI.DotNetStack.DevelopmentTests.Library.Models;
using FFCEI.DotNetStack.MessageBroker;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;

namespace FFCEI.DotNetStack.DevelopmentTests.Library.MessageHandlers;

public sealed class BlogRowMessageHandler
   : MessageHandler<BlogRow>
{
    private static int _received;

    public static void IncrementReceived() => _received++;
    public static int Received => _received;

    public override Task HandleMessageAsync(BlogRow message, IMessageEnvelopeAttributes envelopeAttributes, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Received -> Envelope Attributes: {JsonSerializer.FormattedForScreenInstance.SerializedToString(envelopeAttributes)}\nBlogRow: {JsonSerializer.FormattedForScreenInstance.SerializedToString(message)}");

        ++_received;

        if (_received == 2)
        {
            Console.WriteLine("");
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("Messages received...");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
        }

        return Task.CompletedTask;
    }
}
