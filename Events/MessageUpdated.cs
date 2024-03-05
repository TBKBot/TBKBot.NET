using DSharpPlus.EventArgs;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TBKBot.Utils;

public class MessageUpdateHandler
{
    private readonly DiscordClient _client;

    public MessageUpdateHandler(DiscordClient client)
    {
        _client = client;
        _client.MessageUpdated += OnMessageUpdated;
    }

    public async Task OnMessageUpdated(DiscordClient s, MessageUpdateEventArgs e)
    {
        var logger = new Logging();

        if (e.Message == null || e.MessageBefore == null)
        {
            return;
        }

        if (e.Message.Author.IsBot)
        {
            return;
        }

        if (e.MessageBefore.Content != e.Message.Content)
        {
            await logger.LogMsgEdit(e.MessageBefore, e.Message);
        }
    }
}