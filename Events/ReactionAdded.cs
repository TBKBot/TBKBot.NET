using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBKBot.Models;
using TBKBot.Utils;

public class ReactionAddHandler
{
    private readonly DiscordClient _client;

    public ReactionAddHandler(DiscordClient client)
    {
        _client = client;
        _client.MessageReactionAdded += OnReactAdd;
    }

    public async Task OnReactAdd(DiscordClient s, MessageReactionAddEventArgs e)
    {
    
    }
}