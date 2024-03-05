using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace TBKBot.Events
{
    public class MemberJoinHandler
    {
        private readonly DiscordClient _client;

        public MemberJoinHandler(DiscordClient client)
        {
            _client = client;
            _client.GuildMemberAdded += OnMemberJoin;
        }

        public async Task OnMemberJoin(DiscordClient s, GuildMemberAddEventArgs e)
        {
            var welcome_channel = e.Guild.GetChannel(737683342371061850);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Member Joined",
                Description = $"Welcome {e.Member.Mention} to **{e.Guild.Name}**.",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = e.Member.AvatarUrl,
                    Width = 128,
                    Height = 128
                }
            };

            await welcome_channel.SendMessageAsync(embed);
        }
    }
}
