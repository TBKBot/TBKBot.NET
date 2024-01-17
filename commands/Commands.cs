using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace TBKBot.commands
{
    public class Commands : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            var ctxMsg = ctx.Message;
            var pingMsg = await ctx.RespondAsync("Pinging...");

            var calc = pingMsg.Timestamp.ToUnixTimeMilliseconds() - ctxMsg.Timestamp.ToUnixTimeMilliseconds(); // calculates time between the ctx message sent and the bot editing the message

            await pingMsg.ModifyAsync($":ping_pong: Pong!\nBot latency: {calc}ms\nAPI latency: {Program.Client.Ping}ms");
        }

        [Command("role")]
        [RequirePermissions(Permissions.ManageRoles)]
        public async Task Role(CommandContext ctx, DiscordMember member, DiscordRole role)
        {
            if (!ctx.Member.Permissions.HasPermission(DSharpPlus.Permissions.ManageRoles))
            {
                await ctx.RespondAsync("You do not have the `MANAGE_ROLES` permission in this server");
            }

            if (member.Roles.Contains(role)) 
            {
                await member.RevokeRoleAsync(role);
                await ctx.RespondAsync("Role revoked.");
            }
            else
            {
                await member.GrantRoleAsync(role);
                await ctx.RespondAsync("Role granted.");
            }
        }

        [Command("guess")]
        public async Task GuessNumber(CommandContext ctx)
        {
            Random rnd = new Random();

            var randomNumber = rnd.Next(1,10);

            await ctx.RespondAsync("Pick a number from 1 to 10");

            var interactivity = Program.Client.GetInteractivity();

            var messageToRetrieve = await interactivity.WaitForMessageAsync(message => message.Author == ctx.User);

            if (messageToRetrieve.Result.Content == randomNumber.ToString())
            {
                await ctx.Channel.SendMessageAsync($":tada: The number was **{randomNumber}**. You guessed right!");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"The number was **{randomNumber}**. You guessed wrong.");
            }
        }

        [Command("access")]
        public async Task GiveAccess(CommandContext ctx, DiscordMember member)
        {
            var role = ctx.Guild.GetRole(1193485223128727552);

            await member.GrantRoleAsync(role);

            await ctx.Channel.SendMessageAsync($"> Added {member.Mention} to this category.");
        }

        [Command("rng")]
        public async Task Rng(CommandContext ctx)
        {
            ulong id = ctx.User.Id;

            int seed1 = (int)(id & uint.MaxValue);
            int seed2 = (int)(id >> 32);

            Random rng = new Random(seed1 ^ seed2);

            await ctx.RespondAsync($"{rng.Next(0, 100)}%");
        }

        [Command("snipe")]
        public async Task Snipe(CommandContext ctx)
        {
            var msg = Program.snipeMsg;

            if (msg != null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.Username}** has sniped a message from **{msg.Author.Username}**!",
                    Description = msg.Content
                };

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await ctx.RespondAsync("There was nothing to snipe.");
            }
        }
        
        [Command("snipeedit")]
        public async Task SnipeEdit(CommandContext ctx) // snipeedit needs to be reworked
        {
            /*
            var msg = Program.snipeMsgEdit;

            if (msg != null)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"**{ctx.Member.Username}** has sniped an edited message from **{msg[0].Author.Username}**!",
                    Description = $"Original: {msg[0].Content}\nEdited: {msg[1].Content}"
                };

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await ctx.RespondAsync("There was nothing to snipe.");
            }

            await ctx.Channel.SendMessageAsync(msg + "\n");
            */

            await ctx.RespondAsync("This command is under development");
        }

        [Command("8ball")]
        public async Task Eightball(CommandContext ctx, [RemainingText] string message)
        {
            string[] answers =
                {
                "It is certain", "It is decidedly so", "Without a doubt", "Yes definitely", "You may rely on it",
                "As I see it, yes", "Most likely", "Outlook good", "Yes", "Signs point to yes", "Reply hazy, try again", "Ask again later",
                "Better not tell you now", "Cannot predict now", "Concentrate and ask again", "Don’t count on it", "My reply is no",
                "My sources say no", "Outlook not so good", "Very doubtful"
                };

            Random rand = new Random();

            int index = rand.Next(answers.Length);

            await ctx.RespondAsync($":8ball: {answers[index]}");
        }

        [Command("roll")]
        public async Task Roll(CommandContext ctx, int sides)
        {
            if (sides < 2 | sides > 100)
            {
                await ctx.RespondAsync("You can only roll a dice with 2-100 sides!");
                return;
            }

            var msg = await ctx.RespondAsync($":game_die: You rolled a **D{sides}** ...");

            var rng = new Random().Next(2, sides);

            await msg.ModifyAsync(msg.Content + $"\n\nIt landed on **{rng}** !");
        }
    }
}
