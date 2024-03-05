using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBKBot.Events
{
    public class MemberLeaveHandler
    {
        private readonly DiscordClient _client;

        public MemberLeaveHandler(DiscordClient client)
        {
            _client = client;
            _client.GuildMemberRemoved += OnMemberLeave;
        }

        public async Task OnMemberLeave(DiscordClient s, GuildMemberRemoveEventArgs e)
        {
            var welcome_channel = e.Guild.GetChannel(737683342371061850);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Member Left",
                Description = $"Sorry to see you leave, {e.Member.Username}",
            };

            await welcome_channel.SendMessageAsync(embed);
        }
    }
}
