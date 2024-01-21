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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Collections.Generic;
using Tenor;
using static System.Net.Mime.MediaTypeNames;

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

        [Command("sayrandom")]
        [Aliases("sayrand")]
        public async Task SayRandom(CommandContext ctx, [RemainingText] string prompt)
        {
            if (prompt == null)
            {
                var embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Formatting guide",
                    Description = "`{num1-num2}`: Gives a random number in the range\n`{choice1 | choice2 | choice3}`: Gives a random following choice"
                };

                var embed = embedBuilder.Build();

                await ctx.RespondAsync(embed: embed);

                return;
            }

            prompt = GenerateResult(prompt);

            var messageBuilder = new DiscordMessageBuilder().WithContent(prompt);

            await ctx.RespondAsync(messageBuilder);
        }

        private string GenerateResult(string prompt)
        {
            Regex regex = new Regex(@"\{(.*?)\}");
            MatchCollection matches = regex.Matches(prompt);

            Random random = new Random();

            foreach (Match match in matches)
            {
                string[] options = match.Groups[1].Value.Split('|');

                // Check if the option is a random number range {num1-num2}
                if (options.Length == 1 && options[0].Contains("-"))
                {
                    string[] range = options[0].Split('-');
                    int min = int.Parse(range[0].Trim());
                    int max = int.Parse(range[1].Trim());
                    int randomNumber = random.Next(min, max + 1);

                    prompt = prompt.Replace(match.Value, randomNumber.ToString());
                }
                else
                {
                    string selectedOption = options[random.Next(options.Length)].Trim();

                    if (selectedOption.Contains("{"))
                    {
                        // If the selected option contains nested options, recursively generate the result
                        selectedOption = GenerateResult(selectedOption);
                    }

                    prompt = prompt.Replace(match.Value, selectedOption);
                }
            }

            return prompt;
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

        [Command("melt")]
        public async Task Melt(CommandContext ctx)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz";

            Random random = new Random();

            int length = random.Next(4, 20);

            var output = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            await ctx.RespondAsync(output);
        }

        [Command("hug")]
        public async Task Hug(CommandContext ctx, DiscordMember member)
        {
            string[] gifs = { "https://media1.tenor.com/m/kCZjTqCKiggAAAAd/hug.gif", "https://media1.tenor.com/m/TsEh_PJhTKwAAAAd/pjsk-pjsk-anime.gif", "https://media1.tenor.com/m/uiak6BECN_sAAAAd/emunene-emu.gif",
            "https://media1.tenor.com/m/9e1aE_xBLCsAAAAd/anime-hug.gif", "https://media1.tenor.com/m/FyR2BudmUGAAAAAd/kumirei.gif" };

            int index = new Random().Next(gifs.Length);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Member.DisplayName} hugs {member.DisplayName}",
                ImageUrl = gifs[index]
            };

            var embed = embedBuilder.Build();

            await ctx.RespondAsync(embed);
        }

        [Command("pat")]
        public async Task Pat(CommandContext ctx, DiscordMember member)
        {
            string[] gifs = { "https://media1.tenor.com/m/E6fMkQRZBdIAAAAd/kanna-kamui-pat.gif", "https://media1.tenor.com/m/7xrOS-GaGAIAAAACd/anime-pat-anime.gif", "https://media1.tenor.com/m/OGnRVWCps7IAAAAd/anime-head-pat.gif",
            "https://media1.tenor.com/m/YMRmKEdwZCgAAAAd/anime-hug-anime.gif", "https://media1.tenor.com/m/xvwMZvxTQAQAAAAd/pat.gif" };

            int index = new Random().Next(gifs.Length);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Member.DisplayName} pats {member.DisplayName}",
                ImageUrl = gifs[index]
            };

            var embed = embedBuilder.Build();

            await ctx.RespondAsync(embed);
        }

        [Command("coin")]
        public async Task Coin(CommandContext ctx)
        {
            var message = await ctx.RespondAsync("Throwing coin...");

            Random rnd = new Random();

            var index = rnd.Next(0, 1);

            if (index == 0 )
            {
                await message.ModifyAsync("The coin landed on **heads**!");
            }
            else if (index == 1)
            {
                await message.ModifyAsync("The coin landed on **tails**!");
            }
        }
    }
}
