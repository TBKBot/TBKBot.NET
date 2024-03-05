using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using TBKBot.Utils;

public class MessageDeletionHandler
{
    private readonly DiscordClient _client;

    public Dictionary<ulong, DiscordMessage> snipeableMessages;

    public MessageDeletionHandler(DiscordClient client)
    {
        _client = client;
        snipeableMessages = new Dictionary<ulong, DiscordMessage>();

        _client.MessageDeleted += OnMessageDeleted;
    }

    public async Task OnMessageDeleted(DiscordClient s, MessageDeleteEventArgs e)
    {
        var logger = new Logging();

        if (e.Message == null)
        {
            Console.WriteLine("Message is null in MessageDeleteEventArgs. Message might not have been cached by the bot.");
            return;
            // Message is null, nothing to process
        }

        if (e.Message.Author == null)
            return;
        if (e.Message.Author.IsBot)
            return;

        await logger.LogMsgDeletion(e.Message);

        // Store the last deleted message for the channel
        snipeableMessages[e.Channel.Id] = e.Message;

        // Use Task.Run to execute the delay and message removal on a separate thread
        await Task.Run(async () =>
        {
            var thread_msg = e.Message;
            var thread_channel = e.Channel.Id;

            Console.WriteLine($"Thread {Task.CurrentId} running. Containing message {thread_msg.Content}");

            // Delay for 60 seconds
            await Task.Delay(30000);

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

    public DiscordMessage GetLastDeletedMessage(ulong channelId)
    {
        if (snipeableMessages.ContainsKey(channelId))
        {
            return snipeableMessages[channelId];
        }
        return null;
    }
}