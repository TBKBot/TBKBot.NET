using DSharpPlus.EventArgs;
using DSharpPlus;
using TBKBot.Utils;
using DSharpPlus.Entities;

public class MessageUpdateHandler
{
    private readonly DiscordClient _client;

    public Dictionary<ulong, DiscordMessage> snipeableMessages;

    public MessageUpdateHandler(DiscordClient client)
    {
        _client = client;
        _client.MessageUpdated += OnMessageUpdated;

        snipeableMessages = new Dictionary<ulong, DiscordMessage>();
    }

    public async Task OnMessageUpdated(DiscordClient s, MessageUpdateEventArgs e)
    {
        var logger = new Logging();

        if (e.Message == null || e.MessageBefore == null)
        {
            Console.WriteLine("Message is null in MessageUpdateEventArgs. Message might not have been cached by the bot.");
            return;
            // Message is null, nothing to process
        }

        if (e.Message.Author == null)
            return;
        if (e.Message.Author.IsBot)
            return;
        if (e.MessageBefore.Content == e.Message.Content)
            return;

        await logger.LogMsgEdit(e.MessageBefore, e.Message);

        snipeableMessages[e.Channel.Id] = e.Message;

        await Task.Run(async () =>
        {
            var thread_msg = e.Message;
            var thread_channel = e.Channel.Id;

            Console.WriteLine($"Thread {Task.CurrentId} running. Containing message ID {thread_msg.Id}");

            // Delay for 60 seconds
            await Task.Delay(60000);

            // After the delay, remove the message from the dictionary
            if (thread_msg == snipeableMessages[thread_channel])
            {
                Console.WriteLine("Thread removed message");
                snipeableMessages.Remove(e.Channel.Id);
            }
            else
            {
                Console.WriteLine("Thread skipped removal");
            }
        });
    }

    public DiscordMessage GetLastUpdatedMessage(ulong channelId)
    {
        if (snipeableMessages.ContainsKey(channelId))
        {
            return snipeableMessages[channelId];
        }
        return null;
    }
}