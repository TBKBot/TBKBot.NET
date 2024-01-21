using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TBKBot.other
{
    internal class Logging
    {
        public static ulong logChannel = 737683342371061850;
        public async Task LogDeletion(DiscordMessage msg)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = $"Deleted message in #{msg.Channel.Name}";
            builder.Timestamp = DateTime.Now;
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor();
            builder.Author.Name = $"From {msg.Author.Username}";
            builder.Author.IconUrl = msg.Author.AvatarUrl;
            builder.Description = $"**Message Content**\n{msg.Content}";

            DiscordChannel channel = await Program.Client.GetChannelAsync(logChannel);

            await channel.SendMessageAsync(embed: builder.Build());
        }

        public async Task LogEdit(DiscordMessage beforeMsg, DiscordMessage msg)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.Title = $"Edited message in #{msg.Channel.Name}";
            builder.Timestamp = DateTime.Now;
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor();
            builder.Author.Name = $"From {msg.Author.Username}";
            builder.Author.IconUrl = msg.Author.AvatarUrl;
            builder.Description = $"**Before:**\n{beforeMsg.Content}\n\n**After:**\n{msg.Content}";

            DiscordChannel channel = await Program.Client.GetChannelAsync(logChannel);

            await channel.SendMessageAsync(embed: builder.Build());
        }
    }

}