using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBKBot.Events
{
    public class ComponentInteractionHandler
    {
        private readonly DiscordClient _client;

        public ComponentInteractionHandler(DiscordClient client)
        {
            _client = client;
            _client.ComponentInteractionCreated += OnButtonClicked;
        }

        public async Task OnButtonClicked(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            switch (e.Id)
            {
                case "suggestion_yes":
                    await SendPollToChannel(704465197297041459, e);

                    await e.Message.DeleteAsync();

                    break;

                case "suggestion_no":
                    await e.Interaction.CreateResponseAsync(
                        InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder()
                        {
                            Content = "Suggestion has been deleted.",
                            IsEphemeral = true,
                        });

                    await e.Message.DeleteAsync();

                    break;

                case "suggestion_staff":
                    await SendPollToChannel(840063186052972544, e);

                    await e.Message.DeleteAsync();

                    break;

                case "suggestion_public":
                    await SendPollToChannel(1191831277955989574, e);

                    await e.Message.DeleteAsync();

                    break;
            }
        }

        private async Task SendPollToChannel(ulong id, ComponentInteractionCreateEventArgs e)
        {
            var staffVoteChannel = e.Guild.GetChannel(id);

            await e.Interaction.CreateResponseAsync(
                InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                {
                    Content = $"Suggestion sent to {staffVoteChannel.Mention}!",
                    IsEphemeral = true,
                });

            var emojis = new List<DiscordEmoji> {
                DiscordEmoji.FromGuildEmote(Program.Client, 774965290957406259),
                DiscordEmoji.FromGuildEmote(Program.Client, 774965217376731147),
                DiscordEmoji.FromGuildEmote(Program.Client, 774965378275082251),
                DiscordEmoji.FromGuildEmote(Program.Client, 774965329674895370)
            };

            var embed = e.Message.Embeds[0];

            var description = embed.Description;

            if (embed.Description.Length > 255)
            {
                description = description.Substring(0, 255) + "-";
            }

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = description,
                Description = $"{emojis[0]} Yes\n{emojis[1]} No\n{emojis[2]} Neutral\n{emojis[3]} Maybe",
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = embed.Author.Name,
                    IconUrl = embed.Author.IconUrl.ToString(),
                    Url = embed.Author.Url.ToString()
                }
            };

            var msg = await staffVoteChannel.SendMessageAsync(pollEmbed);

            foreach (var emoji in emojis)
            {
                await msg.CreateReactionAsync(emoji);
            }
        }
    }
}
