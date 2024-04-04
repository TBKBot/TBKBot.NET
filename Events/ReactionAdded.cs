using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using TBKBot;
using TBKBot.Data;
using TBKBot.Models;

public class ReactionAddHandler
{
    private readonly DiscordClient _client;
    private DBEngine DB = Program.DBEngine;

    public ReactionAddHandler(DiscordClient client)
    {
        _client = client;
        _client.MessageReactionAdded += OnReactAdd;
    }

    public async Task OnReactAdd(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (eventArgs.Emoji.GetDiscordName() == ":star:")
        {
            var starboardChannel = eventArgs.Guild.GetChannel(737683342371061850);

            var message = await eventArgs.Channel.GetMessageAsync(eventArgs.Message.Id);

            var starReaction = await eventArgs.Message.GetReactionsAsync(eventArgs.Emoji);

            if (starReaction.Count < 3)
                return;

            var data = await DB.LoadStarMessageAsync(message.Id);
            if (data == null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Author = new DiscordEmbedBuilder.EmbedAuthor
                    {
                        Name = message.Author.Username,
                        IconUrl = message.Author.AvatarUrl,
                        Url = message.JumpLink.ToString()
                    },
                    Description = message.Content,
                    Color = DiscordColor.Yellow,
                    ImageUrl = message.Attachments.Count > 0 ? message.Attachments[0].Url : null,
                    Timestamp = message.CreationTimestamp
                };

                var boardMessage = await starboardChannel.SendMessageAsync($":star: **{starReaction.Count}** {eventArgs.Message.JumpLink}", embed);

                var starboardData = new StarMessage
                {
                    Id = message.Id,
                    Stars = starReaction.Count,
                    AuthorId = message.WebhookMessage ? null : message.Author.Id,
                    ChannelId = message.ChannelId,
                    Content = message.Content,
                    BoardMessageId = boardMessage.Id
                };

                await DB.SaveStarMessageAsync(starboardData);
                return;
            }

            data.Stars = starReaction.Count;

            await DB.SaveStarMessageAsync(data);

            var boardMsg = await starboardChannel.GetMessageAsync(data.BoardMessageId);

            await boardMsg.ModifyAsync($":star2: **{starReaction.Count}** {eventArgs.Message.JumpLink}");
            
        }
    }
}