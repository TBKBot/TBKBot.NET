using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using TBKBot.Data;

namespace TBKBot.Utils
{
    internal class Logging
    {
        public static ulong logChannelId = 737683342371061850;
        public async Task LogMsgDeletion(DiscordMessage msg)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Deleted message in #{msg.Channel.Name}",
                Timestamp = DateTime.Now,
                Color = Program.EmbedColor,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"From {msg.Author.Username}",
                    IconUrl = msg.Author.AvatarUrl,
                },
            };

            if (msg.Reference != null)
            {
                embed.Description = $"**Replying to [message]({msg.Reference.Message.JumpLink})**";
            }

            embed.AddField("Message content:", msg.Content, false);

            DiscordChannel channel = await Program.Client.GetChannelAsync(logChannelId);

            await channel.SendMessageAsync(embed: embed.Build());
        }

        public async Task LogMsgEdit(DiscordMessage beforeMsg, DiscordMessage msg)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Edited message in #{msg.Channel.Name}",
                Timestamp = DateTime.Now,
                Color = Program.EmbedColor,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = $"From {msg.Author.Username}",
                    IconUrl = msg.Author.AvatarUrl,
                },
            };

            if (msg.Reference != null)
            {
                embed.Description = $"**Replying to [message]({msg.Reference.Message.JumpLink})**";
            }

            embed.AddField("Before:", beforeMsg.Content, false);
            embed.AddField("After:", msg.Content, false);

            DiscordChannel channel = await Program.Client.GetChannelAsync(logChannelId);

            await channel.SendMessageAsync(embed);
        }

        public async Task LogMimic(InteractionContext ctx, DiscordMessage mimicMsg, DiscordUser mimiced_user, string content)
        {
            var embed = new DiscordEmbedBuilder
            {
                Description = $"{ctx.User.Mention} mimiced {mimiced_user.Mention} in {mimicMsg.JumpLink}\n\n\"{content}\"",
                Timestamp = DateTime.Now,
                Color = Program.EmbedColor
            };

            var logChannel = await Program.Client.GetChannelAsync(logChannelId);

            var DBEngine = new DBEngine();

            var mimicData = new MimicMessage
            {
                Id = mimicMsg.Id,
                Content = mimicMsg.Content,
                ImpersonatorId = ctx.User.Id,
                ImpersonatorUsername = ctx.User.Username,
                CreationTime = DateTime.Now
            };

            await DBEngine.SaveMimicMessageAsync(mimicData);

            await logChannel.SendMessageAsync(embed);
        }
    }
}