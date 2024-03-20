using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using MongoDB.Driver.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using TBKBot.Data;
using TBKBot.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SharpCompress.Common;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text;

namespace TBKBot.commands
{
    public class PrefixCommands : BaseCommandModule
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
                return;
            }

            if (ctx.Member.Hierarchy <= member.Hierarchy)
            {
                await ctx.RespondAsync("Provided member role level is too high");
                return;
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

        /*
        [Command("guess")]
        public async Task GuessNumber(CommandContext ctx)
        {
            Random rnd = new Random();

            var randomNumber = rnd.Next(1, 10);

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
        */

        [Command("sayrandom")]
        [Aliases("sayrand", "say")]
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
            var deletionHandler = Program._messageDeletionHandler;
            var snipeMsg = deletionHandler.GetLastDeletedMessage(ctx.Channel.Id);

            if (snipeMsg == null)
            {
                await ctx.RespondAsync("There was nothing to snipe.");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"**{ctx.Member.Username}** has sniped a message from **{snipeMsg.Author.Username}**!",
                Description = snipeMsg.Content
            };

            await ctx.RespondAsync(embed);
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
                Description = $"{ctx.Member.Mention} gives {member.Mention} a hug :people_hugging:",
                ImageUrl = gifs[index]
            };

            var embed = embedBuilder.Build();

            await ctx.RespondAsync(embed);
        }

        [Command("pat")]
        public async Task Pat(CommandContext ctx, DiscordMember member)
        {
            string[] gifs = { "https://media1.tenor.com/m/E6fMkQRZBdIAAAAd/kanna-kamui-pat.gif", "https://media1.tenor.com/m/7xrOS-GaGAIAAAAd/anime-pat-anime.gif", "https://media1.tenor.com/m/OGnRVWCps7IAAAAd/anime-head-pat.gif",
            "https://media1.tenor.com/m/YMRmKEdwZCgAAAAd/anime-hug-anime.gif", "https://media1.tenor.com/m/xvwMZvxTQAQAAAAd/pat.gif" };

            int index = new Random().Next(gifs.Length);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Description = $"{ctx.Member.Mention} headpats {member.Mention} :heart:",
                ImageUrl = gifs[index]
            };

            var embed = embedBuilder.Build();

            await ctx.RespondAsync(embed);
        }

        [Command("coin")]
        public async Task Coin(CommandContext ctx, string prompt, int bet = 0)
        {
            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            if (bet < 0)
            {
                await ctx.RespondAsync("Invalid bet amount. Please enter a positive value.");
                return;
            }

            var db = new DBEngine("tbkbot");
            var data = await db.LoadMemberAsync(ctx.Member.Id);

            if (bet > data.Money)
            {
                await ctx.RespondAsync($"You don't have enough to bet: {data.Money:N0} {coinEmoji}");
                return;
            }

            var message = await ctx.RespondAsync("Throwing coin...");

            Random rnd = new Random();
            var result = rnd.Next(0, 2) == 0 ? "heads" : "tails";
            bool guessedCorrectly = prompt.ToLower() == result;
            int outcome = guessedCorrectly ? bet : -bet;

            data.Money = Math.Max(0, data.Money + outcome);
            await db.SaveMemberAsync(data);

            var embed = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Member.Username, iconUrl: ctx.Member.AvatarUrl)
                .WithColor(guessedCorrectly && bet > 0 ? DiscordColor.Green : DiscordColor.Red);

            string response = $"The coin landed on {result}. ";
            if (bet > 0)
            {
                response += guessedCorrectly ? "You guessed correctly!" : "You guessed incorrectly";
                var embedResponse = $"{(guessedCorrectly ? "+" : "-")}{bet:N0} {coinEmoji} (Wallet: {data.Money:N0} {coinEmoji})";

                embed.WithDescription(embedResponse);

                await message.ModifyAsync(response);
                await ctx.Channel.SendMessageAsync(embed);
            }
            else
            {
                await message.ModifyAsync(response);
            }
        }

        /*
        [Command("help")]
        public async Task Help(CommandContext ctx, string command = null)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Command help",
                Description = "This is the command help text"
            };

            if (command != null)
            {
                Program.Client.GetCommandsNext();
            }

            await ctx.RespondAsync(embed);
        }
        */

        [Command("avatar")]
        public async Task Avatar(CommandContext ctx, DiscordMember member = null)
        {
            if (member == null)
            {
                member = ctx.Member;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{member?.Username}",
                Description = $"**[Avatar URL]({member?.AvatarUrl})**",
                ImageUrl = member.AvatarUrl,
                Color = Program.EmbedColor
            };

            await ctx.RespondAsync(embed);
        }


        [Command("balance")]
        [Aliases("bal")]
        public async Task Balance(CommandContext ctx, DiscordMember member = null)
        {
            if (member == null)
            {
                member = ctx.Member;
            }

            var db = new DBEngine("tbkbot");
            var data = await db.LoadMemberAsync(member.Id);

            if (data == null)
            {
                await ctx.RespondAsync("Something wrong occured: Failed to retrieve member data");
                return;
            }

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            await ctx.RespondAsync($"Wallet: {data.Money:N0} {coinEmoji}\nBank: {data.Bank:N0} {coinEmoji}");
        }


        [Command("donate")]
        public async Task DonateToBot(CommandContext ctx, int amount = 0)
        {
            var db = new DBEngine("tbkbot");
            var data = await db.LoadMemberAsync(ctx.User.Id);

            if (amount == 0)
            {
                await ctx.RespondAsync("Command usage: `t.donate <amount>`");
                return;
            }

            // if the amount is more than the users balance, use the users whole balance
            if (data.Money < amount)
            {
                amount = data.Money;
            }

            data.Money -= amount;

            await ctx.RespondAsync($"**{ctx.User.Username}** has donated ${amount.ToString("N0")} to TBKBot. Thank you!");

            await db.SaveMemberAsync(data);
        }



        [Command("leaderboard")]
        [Aliases("lb")]
        public async Task MoneyLeaderboard(CommandContext ctx, int page = 1)
        {
            var db = new DBEngine("tbkbot");
            var members = await db.GetAllMembersAsync();

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            var memberDict = new Dictionary<string, int>();

            foreach (var member in members)
            {
                string idString = member.Id.ToString();
                memberDict.Add(idString, member.Money + member.Bank);
            }

            // Sort the dictionary by value in descending order
            var sortedMembers = memberDict.OrderByDescending(pair => pair.Value);

            // Calculate the total number of pages
            int maxPages = (int)Math.Ceiling((double)sortedMembers.Count() / 10);

            // Ensure the provided page number is within the valid range
            page = Math.Max(1, Math.Min(page, maxPages));

            // Skip the appropriate number of members based on the page number
            var filteredMembers = sortedMembers.Skip((page - 1) * 10);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Money Leaderboard",
                Color = new DiscordColor("#FFD700"),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Page: {page}/{maxPages}"
                }
            };

            int i = 1 + (page - 1) * 10;
            foreach (var pair in filteredMembers.Take(10)) // Take only the top 10 members
            {
                embed.Description += $"{i} - <@{pair.Key}> - {pair.Value:N0} {coinEmoji}\n";
                i++;
            }

            // Send the leaderboard embed
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("give")]
        public async Task GiveMoney(CommandContext ctx, DiscordMember member, int amount)
        {
            var db = new DBEngine("tbkbot");

            bool exchange = true;

            if (ctx.Member.Id == 425661467904180224)
            {
                exchange = false;
            }

            var benefactor = await db.LoadMemberAsync(ctx.Member.Id);

            if (exchange)
            {
                if (benefactor.Money < amount)
                {
                    return;
                }
                benefactor.Money -= amount;

                await db.SaveMemberAsync(benefactor);
            }

            var beneficiary = await db.LoadMemberAsync(member.Id);

            beneficiary.Money += amount;

            await db.SaveMemberAsync(beneficiary);

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            await ctx.RespondAsync($"{amount:N0} {coinEmoji} has been given to {member.Username}. ({benefactor.Money:N0} {coinEmoji})");
        }

        [Command("reactpick")]
        public async Task ReactionPick(CommandContext ctx, DiscordEmoji emoji)
        {
            // Get the message
            var message = ctx.Message.ReferencedMessage;
            if (message == null)
            {
                await ctx.RespondAsync("Message not found.");
                return;
            }

            // Get the list of users who reacted with the randomly selected reaction
            var users = await message.GetReactionsAsync(emoji);

            // Check if there are any users who reacted with the selected reaction
            if (users.Count == 0)
            {
                await ctx.RespondAsync("No users found who reacted with the randomly selected reaction.");
                return;
            }

            // Randomly select a user from the list
            var random = new Random();
            var randomUser = users[random.Next(users.Count)];

            // Mention the selected user
            await ctx.RespondAsync($"{randomUser.Mention}");
        }

        private Dictionary<ulong, string> cachedMessages = new Dictionary<ulong, string>();

        [Command("string")]
        public async Task RandomString(CommandContext ctx, DiscordChannel channel = null)
        {
            if (channel == null)
            {
                channel = ctx.Channel;
            }

            var random = new Random();

            var loadingEmoji = DiscordEmoji.FromGuildEmote(Program.Client, 1215487770147950634);

            await ctx.Channel.TriggerTypingAsync();

            string messageContents = "";

            if (cachedMessages.ContainsKey(channel.Id))
            {
                messageContents = cachedMessages[channel.Id];
            }
            else
            {
                var retrievedMessages = await channel.GetMessagesAsync(1000);

                string allMessagesContent = string.Join("\n", retrievedMessages.Select(msg => msg.Content));

                cachedMessages[channel.Id] = allMessagesContent;

                messageContents = allMessagesContent;
            }

            int limit = random.Next(1, 20);

            /*
            string pattern = @"(?:https?|ftp):\/\/[\n\S]+";

            messageContents = Regex.Replace(messageContents, pattern, "");
            */

            // Split the concatenated string into words
            var words = messageContents.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Shuffle the words to get a random order
            words = words.OrderBy(_ => random.Next()).ToArray();

            // Pick random words from the shuffled list
            List<string> randomWords = words.Take(limit).ToList();

            // Join the random words into a string
            string result = string.Join(" ", randomWords);

            // Send the string as a message
            await ctx.RespondAsync(content: result.ToLower());
        }

        [Command("fiveoclock")]
        public async Task FiveoClock(CommandContext ctx)
        {
            DateTime currentTime = DateTime.UtcNow;

            foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
            {
                DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(currentTime, timeZone);
                if (localTime.Hour == 17) // 5 PM
                {
                    string[] displayNameParts = timeZone.DisplayName.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

                    string[] regions = displayNameParts[1].Trim().Split(",");
                    string timeZoneId = displayNameParts[0].Trim();

                    var messages = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, 100);
                    if (messages.Any(x => x.Content.Contains("t.fiveoclock")))
                    {
                        await ctx.RespondAsync($"It's 5 o' clock in: [{regions[0]}](<https://en.wikipedia.org/wiki/{regions[0].Replace(" ", "_")}>) ({timeZoneId})");
                    }
                    else
                    {
                        await ctx.RespondAsync($"It's 5 o' clock in: [{regions[0]}](https://en.wikipedia.org/wiki/{regions[0].Replace(" ", "_")}) ({timeZoneId})");
                    }
                    
                    break;
                }
            }
        }

        [Command("setbirthday")]
        public async Task SetBirthday(CommandContext ctx, string date)
        {
            try
            {
                DateTime dt = DateTime.ParseExact(date, "M/dd", CultureInfo.InvariantCulture);

                dt.ToUniversalTime();

                var db = new DBEngine("tbkbot");

                var data = await db.LoadMemberAsync(ctx.User.Id);

                data.Birthday = dt;

                await db.SaveMemberAsync(data);

                await ctx.RespondAsync($"Your birthday has been saved to {dt:MMMM dd}");
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message);
            }
        }

        [Command("speechbubble")]
        public async Task SpeechBubble(CommandContext ctx, string url)
        {
            if (!url.Contains("https://"))
            {
                await ctx.RespondAsync("No URL provided.");
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Stream stream = await client.GetStreamAsync(url);

                    Stream pngStream;

                    // Open the file stream
                    using (FileStream fileStream = File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Media", "speechbubble.png")))
                    {
                        pngStream = new MemoryStream();
                        await fileStream.CopyToAsync(pngStream);
                        pngStream.Position = 0; // Reset the position to the beginning
                    }

                    using (Image image = Image.Load(stream))
                    using (Image pngImage = Image.Load(pngStream))
                    {
                        // Resize the PNG image to fit the full width of the background image
                        int newWidth = image.Width;
                        int newHeight = (int)(image.Height / 3);
                        pngImage.Mutate(ctx => ctx.Resize(newWidth, newHeight));

                        image.Mutate(ctx => ctx
                            .DrawImage(pngImage, new Point(0, 0), 1f));

                        // Save the resulting image
                        using (MemoryStream resultStream = new MemoryStream())
                        {
                            image.SaveAsJpeg(resultStream); // Save as JPEG
                            resultStream.Position = 0;

                            // Create a DiscordMessageBuilder
                            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                                .WithContent("Here's the file")
                                .AddFile("speechbubble.jpg", resultStream); // Change the file name if needed

                            await ctx.RespondAsync(messageBuilder);
                        }
                    }
                }
            } catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message);
            }
        }

        [Command("kill")]
        public async Task Kill(CommandContext ctx, DiscordMember member)
        {
            if (member == ctx.Member)
            {
                await ctx.RespondAsync($"**{member.DisplayName}** killed themselves. RIP");
                return;
            }

            if (member.IsBot)
            {
                string cmd = member.CreationTimestamp.ToUnixTimeSeconds().ToString() + ".cmd.kill";

                byte[] bytes = Encoding.UTF8.GetBytes(cmd);

                string killcode = Convert.ToBase64String(bytes);

                var loadingEmoji = await ctx.Guild.GetEmojiAsync(1215487770147950634);

                var msg = await ctx.RespondAsync($"BOT DETECTED {loadingEmoji}");

                await msg.ModifyAsync($"BOT DETECTED\nINITIATING KILL PROTOCOL {loadingEmoji}");

                await msg.ModifyAsync($"BOT DETECTED\nINITIATING KILL PROTOCOL\nENTER THE KILL-CODE:");

                var interactivity = Program.Client.GetInteractivity();

                var followupMsg = await interactivity.WaitForMessageAsync(message => message.Author == ctx.User && message.ReferencedMessage == msg);

                if (followupMsg.Result == null)
                {
                    await msg.RespondAsync("PROCESS TIMED OUT.\nTERMINATING KILL PROCESS.");
                    return;
                }

                if (followupMsg.Result.Content == killcode)
                {
                    await followupMsg.Result.RespondAsync($"KILL-CODE INJECTED.\nBOT {member.DisplayName} HAS BEEN TERMINATED.");
                    return;
                }
                else
                {
                    await followupMsg.Result.RespondAsync("KILL-CODE INVALID.\nTERMINATING KILL PROCESS.");
                    return;
                }
            }

            await ctx.RespondAsync($"**{ctx.Member.DisplayName}** killed **{member.DisplayName}**");
        }

        [Command("deposit")]
        [Aliases("dep")]
        public async Task Deposit(CommandContext ctx, string amount)
        {
            var db = new DBEngine("tbkbot");

            var data = await db.LoadMemberAsync(ctx.Member.Id);

            int amountInt;

            if (int.TryParse(amount, out amountInt))
            {
                if (amountInt > data.Money)
                {
                    return;
                }
            }
            else if (amount == "all")
            {
                amountInt = data.Money;
            }
            else
            {
                return;
            }

            data.Bank += amountInt;
            data.Money -= amountInt;

            await db.SaveMemberAsync(data);

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            await ctx.RespondAsync($"{amountInt:N0} {coinEmoji} deposited into the bank. (Wallet: {data.Money:N0} {coinEmoji})");
        }

        [Command("withdraw")]
        [Aliases("with")]
        public async Task Withdraw(CommandContext ctx, int amount)
        {
            var db = new DBEngine("tbkbot");

            var data = await db.LoadMemberAsync(ctx.User.Id);

            if (amount > data.Bank)
            {
                return;
            }

            data.Money += amount;
            data.Bank -= amount;

            await db.SaveMemberAsync(data);

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            await ctx.RespondAsync($"{amount:N0} {coinEmoji} withdrawn from the bank. (Wallet: {data.Money:N0} {coinEmoji})");
        }

        [Command("steal")]
        public async Task Steal(CommandContext ctx, DiscordMember member)
        {
            if (member == ctx.Member)
            {
                return;
            }

            var db = new DBEngine("tbkbot");

            var thief = await db.LoadMemberAsync(ctx.Member.Id);
            var victim = await db.LoadMemberAsync(member.Id);

            TimeSpan cooldownTime = TimeSpan.FromHours(1); // Adjust cooldown time as needed
            var lastStealTime = thief.LastStealTime;

            if (lastStealTime.HasValue && DateTime.UtcNow - lastStealTime.Value < cooldownTime)
            {
                var cooldownEnd = DateTime.UtcNow.Add(cooldownTime);
                var timestamp = new DateTimeOffset(cooldownEnd).ToUnixTimeSeconds();
                await ctx.RespondAsync($"You are on cooldown for stealing. You can steal again <t:{timestamp}:R>.");
                return;
            }

            int randomDivider = new Random().Next(10, 50);

            int amountStolen = (int)(victim.Money / randomDivider);

            amountStolen = amountStolen < 50 ? victim.Money : amountStolen;

            thief.LastStealTime = DateTime.UtcNow;

            if (amountStolen == 0)
            {
                await ctx.RespondAsync("You didn't manage to steal anything.");
                return;
            }

            thief.Money += amountStolen;
            victim.Money -= amountStolen;

            await db.SaveMemberAsync(thief);
            await db.SaveMemberAsync(victim);

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            await ctx.RespondAsync($"Stole {amountStolen:N0} {coinEmoji} from {member.Mention}.");
        }
    }
}