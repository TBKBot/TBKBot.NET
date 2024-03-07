using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBKBot.Data;
using TBKBot.Models;
using TBKBot.Utils;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

public class ReactionAddHandler
{
    private readonly DiscordClient _client;

    public ReactionAddHandler(DiscordClient client)
    {
        _client = client;
        _client.MessageReactionAdded += OnReactAdd;
    }

    public async Task OnReactAdd(DiscordClient client, MessageReactionAddEventArgs eventArgs)
    {
        if (eventArgs.Emoji.GetDiscordName() == ":star2:")
        {
            var starboardChannel = eventArgs.Guild.GetChannel(737683342371061850);

            var starReaction = await eventArgs.Message.GetReactionsAsync(eventArgs.Emoji);

            if (starReaction.Count >= 3) // starboard condition for a message
            {
                var member = (DiscordMember)eventArgs.User;

                var DBEngine = new DBEngine("starboard");

                var data = await DBEngine.LoadStarMessageAsync(eventArgs.Message.Id);
                if (data == null)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Author = new DiscordEmbedBuilder.EmbedAuthor
                        {
                            Name = (member.Nickname == null) ? member.Username : member.Nickname,
                            IconUrl = (member.GuildAvatarUrl == null) ? member.AvatarUrl : member.GuildAvatarUrl,
                        },
                        Description = eventArgs.Message.Content,
                        Color = DiscordColor.Yellow,
                        ImageUrl = (eventArgs.Message.Attachments.Count == 0) ? null : eventArgs.Message.Attachments[0].Url,
                        Timestamp = DateTime.Now
                    };

                    var boardMessage = await starboardChannel.SendMessageAsync($":star2: **{starReaction.Count}** {eventArgs.Message.JumpLink}", embed);

                    var starboardData = new StarMessage
                    {
                        Id = eventArgs.Message.Id,
                        Stars = starReaction.Count,
                        AuthorId = eventArgs.Message.Author.Id,
                        ChannelId = eventArgs.Message.ChannelId,
                        Content = eventArgs.Message.Content,
                        BoardMessageId = boardMessage.Id
                    };

                    await DBEngine.SaveStarMessageAsync(starboardData);
                    return;
                }

                data.Stars = starReaction.Count;

                await DBEngine.SaveStarMessageAsync(data);

                var boardMsg = await starboardChannel.GetMessageAsync(data.BoardMessageId);

                await boardMsg.ModifyAsync($":star2: **{starReaction.Count}** {eventArgs.Message.JumpLink}");
            }
        }
    }
}