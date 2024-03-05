using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBKBot.Data;
using TBKBot.Utils;

namespace TBKBot.Commands.Slash
{
    internal class SlashCommands : ApplicationCommandModule
    {
        [SlashCommand("ping", "Checks bot latency")]
        public async Task Ping(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{Program.Client.Ping}ms"));
        }

        [SlashCommand("mimic", "Create a message mimicking another member")]
        public async Task Mimic(InteractionContext ctx,
                                [Option("member", "The member to mimic")] DiscordUser user,
                                [Option("message", "Message to send")] string content)
        {
            await ctx.DeferAsync(ephemeral: true);

            var webhook = new DiscordWebhookBuilder()
                .WithContent("Message sent");

            var member = await ctx.Guild.GetMemberAsync(user.Id);

            try
            {
                // Create a webhook for the channel
                var existingWebhooks = await ctx.Channel.GetWebhooksAsync();
                var existingWebhook = existingWebhooks.FirstOrDefault(w => w.Name == "MimicWebhook");

                if (existingWebhook == null)
                {
                    existingWebhook = await ctx.Channel.CreateWebhookAsync("MimicWebhook");
                }


                // Build webhook message content
                var webhookMessage = new DiscordWebhookBuilder()
                    .WithUsername(member.DisplayName)
                    .WithAvatarUrl(member.AvatarUrl)
                    .WithContent(content);

                // Send message via webhook
                var mimicMsg = await existingWebhook.ExecuteAsync(webhookMessage);

                await ctx.EditResponseAsync(webhook);

                var logger = new Logging();

                await logger.LogMimic(ctx, mimicMsg, user, content);

                return;
            }
            catch (Exception ex)
            {
                webhook.Content = $"Something went wrong: {ex}";

                await ctx.EditResponseAsync(webhook);
            }
        }

        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Who mimicked?")]
        public async Task WhoMimic(ContextMenuContext ctx)
        {
            await ctx.DeferAsync(ephemeral: true);

            var DBEngine = new DBEngine("tbkbot");

            var webhook = new DiscordWebhookBuilder();

            var msgData = await DBEngine.FindMimicMessageAsync(ctx.TargetMessage.Id);

            if (msgData == null)
            {
                webhook.Content = "Mimic message was not found in the database.";

                await ctx.EditResponseAsync(webhook);

                return;
            }

            webhook.Content = $"**{msgData.ImpersonatorUsername}** mimicked this [message](<{ctx.TargetMessage.JumpLink}>).";

            await ctx.EditResponseAsync(webhook);
        }
    }
}
