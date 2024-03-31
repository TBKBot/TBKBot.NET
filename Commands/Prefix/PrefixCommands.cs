using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MongoDB.Driver.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using TBKBot.Data;
using ZstdSharp.Unsafe;

namespace TBKBot.commands
{
    public class PrefixCommands : BaseCommandModule
    {
        public Random Rng { private get; set; }

        [Command("ping"), Description("Checks bot and API latency")]
        public async Task Ping(CommandContext ctx)
        {
            var ctxMsg = ctx.Message;
            var pingMsg = await ctx.RespondAsync("Pinging...");

            var calc = pingMsg.Timestamp.ToUnixTimeMilliseconds() - ctxMsg.Timestamp.ToUnixTimeMilliseconds(); // calculates time between the ctx message sent and the bot editing the message

            await pingMsg.ModifyAsync($":ping_pong: Pong!\nBot latency: {calc}ms\nAPI latency: {Program.Client.Ping}ms");
        }

        [Command("role"), Description("Gives/removes member role"), GuildOnly]
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

        [Command("say"), Description("Formats text")]
        [Aliases("sayrand", "sayrandom")]
        public async Task SayRandom(CommandContext ctx, [RemainingText] string prompt)
        {
            if (prompt == null)
            {
                var embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Formatting guide",
                    Description = "`{%}`: Gets a random percentage (0-100)\n" +
                                  "`{num1 num2}`: Gives a random number in the range\n" +
                                  "`{\"choice1\" \"choice2\" \"choice3\"}`: Gives a random following choice"
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
            Regex regex = new Regex(@"\{(?:[^{}]|(?<o>{)|(?<-o>}))*\}");
            MatchCollection matches = regex.Matches(prompt);

            foreach (Match match in matches)
            {
                string matchValue = match.Value;

                // Handle special cases
                if (matchValue == "{%}")
                {
                    int randomNumber = Rng.Next(101);
                    prompt = prompt.Replace(matchValue, randomNumber.ToString());
                }
                else if (matchValue.StartsWith("{\"") && matchValue.EndsWith("\"}"))
                {
                    string content = matchValue.Substring(2, matchValue.Length - 4);
                    string[] stringOptions = content.Split('"', StringSplitOptions.RemoveEmptyEntries);
                    string selectedStringOption = stringOptions[Rng.Next(stringOptions.Length)].Trim();
                    prompt = prompt.Replace(matchValue, selectedStringOption);
                    continue;
                }
                else if (matchValue.StartsWith("{") && matchValue.EndsWith("}"))
                {
                    string content = matchValue.Substring(1, matchValue.Length - 2);
                    string[] numberOptions = content.Split(' ');
                    if (numberOptions.Length == 2)
                    {
                        int min = int.Parse(numberOptions[0].Trim());
                        int max = int.Parse(numberOptions[1].Trim());
                        int randomNumber = Rng.Next(min, max + 1);
                        prompt = prompt.Replace(matchValue, randomNumber.ToString());
                        continue;
                    }
                }

                string[] options = matchValue.Split(' ');

                string selectedOption = options[Rng.Next(options.Length)].Trim();

                prompt = prompt.Replace(matchValue, selectedOption);
            }

            return prompt;
        }


        [Command("snipe"), Description("Snipes last deleted message on this channel")]
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

        [Command("snipeedit"), Description("Snipes last message edit on this channel")]
        public async Task SnipeEdit(CommandContext ctx)
        {
            var updateHandler = Program._messageUpdateHandler;
            var snipeMsg = updateHandler.GetLastUpdatedMessage(ctx.Channel.Id);

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

        [Command("8ball"), Description("Sink the Eight, Seal Your Fate!")]
        public async Task Eightball(CommandContext ctx, [RemainingText, Description("Ask the magic 8-ball")] string message)
        {
            string[] answers =
                {
                "It is certain", "It is decidedly so", "Without a doubt", "Yes definitely", "You may rely on it",
                "As I see it, yes", "Most likely", "Outlook good", "Yes", "Signs point to yes", "Reply hazy, try again", "Ask again later",
                "Better not tell you now", "Cannot predict now", "Concentrate and ask again", "Don’t count on it", "My reply is no",
                "My sources say no", "Outlook not so good", "Very doubtful"
                };

            int index = Rng.Next(answers.Length);

            await ctx.RespondAsync($":8ball: {answers[index]}");
        }

        [Command("roll"), Description("Roll the dice")]
        public async Task Roll(CommandContext ctx, int sides)
        {
            if (sides < 2 | sides > 100)
            {
                await ctx.RespondAsync("You can only roll a dice with 2-100 sides!");
                return;
            }

            var msg = await ctx.RespondAsync($":game_die: You rolled a **D{sides}** ...");

            var side = Rng.Next(2, sides+1);

            await msg.ModifyAsync(msg.Content + $"\n\nIt landed on **{side}** !");
        }

        [Command("melt"), Description("\"cute melt\" - Nikki")]
        public async Task Melt(CommandContext ctx)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            int length = Rng.Next(4, 20);

            var output = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Rng.Next(s.Length)]).ToArray());

            await ctx.RespondAsync(output);
        }

        [Command("hug"), Description("Give a hug!")]
        public async Task Hug(CommandContext ctx, DiscordMember member)
        {
            string[] gifs = { "https://media1.tenor.com/m/kCZjTqCKiggAAAAd/hug.gif", "https://media1.tenor.com/m/TsEh_PJhTKwAAAAd/pjsk-pjsk-anime.gif", "https://media1.tenor.com/m/uiak6BECN_sAAAAd/emunene-emu.gif",
            "https://media1.tenor.com/m/9e1aE_xBLCsAAAAd/anime-hug.gif", "https://media1.tenor.com/m/FyR2BudmUGAAAAAd/kumirei.gif" };

            int index = Rng.Next(gifs.Length + 1);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Description = $"{ctx.Member.Mention} gives {member.Mention} a hug :people_hugging:",
                ImageUrl = gifs[index]
            };

            var embed = embedBuilder.Build();

            await ctx.RespondAsync(embed);
        }

        [Command("pat"), Description("Headpat someone")]
        public async Task Pat(CommandContext ctx, [Description("Person to give a headpat")] DiscordMember member)
        {
            string[] gifs = { "https://media1.tenor.com/m/E6fMkQRZBdIAAAAd/kanna-kamui-pat.gif", "https://media1.tenor.com/m/7xrOS-GaGAIAAAAd/anime-pat-anime.gif", "https://media1.tenor.com/m/OGnRVWCps7IAAAAd/anime-head-pat.gif",
            "https://media1.tenor.com/m/YMRmKEdwZCgAAAAd/anime-hug-anime.gif", "https://media1.tenor.com/m/xvwMZvxTQAQAAAAd/pat.gif" };

            int index = Rng.Next(gifs.Length + 1);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Description = $"{ctx.Member.Mention} headpats {member.Mention} :heart:",
                ImageUrl = gifs[index]
            };

            var embed = embedBuilder.Build();

            await ctx.RespondAsync(embed);
        }

        [Command("coin"), Description("Toss a coin and win bets")]
        public async Task Coin(CommandContext ctx, [Description("Choose heads or tails")] string guess, int bet = 0)
        {
            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            if (bet < 0)
            {
                await ctx.RespondAsync("Invalid bet amount. Please enter a positive value.");
                return;
            }

            var db = new DBEngine();
            var data = await db.LoadMemberAsync(ctx.Member.Id);

            if (bet > data.Money)
            {
                await ctx.RespondAsync($"You don't have enough to bet: {data.Money:N0} {coinEmoji}");
                return;
            }

            var message = await ctx.RespondAsync("Throwing coin...");

            var result = Rng.Next(0, 2) == 0 ? "heads" : "tails";
            bool guessedCorrectly = guess.ToLower() == result;
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

        [Command("avatar"), Description("Show member avatar")]
        public async Task Avatar(CommandContext ctx, [Description("Avatar member")] DiscordMember member = null)
        {
            if (member == null)
            {
                member = ctx.Member;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{member.Username}",
                Description = $"**[Avatar URL]({member.AvatarUrl})**",
                ImageUrl = member.AvatarUrl,
                Color = Program.EmbedColor
            };

            await ctx.RespondAsync(embed);
        }


        [Command("balance"), Description("Check your or another member's balance")]
        [Aliases("bal")]
        public async Task Balance(CommandContext ctx, DiscordMember member = null)
        {
            if (member == null)
            {
                member = ctx.Member;
            }

            var db = new DBEngine();
            var data = await db.LoadMemberAsync(member.Id);

            if (data == null)
            {
                await ctx.RespondAsync("Something wrong occured: Failed to retrieve member data");
                return;
            }

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            await ctx.RespondAsync($"Wallet: {data.Money:N0} {coinEmoji}\nBank: {data.Bank:N0} {coinEmoji}");
        }


        [Command("donate"), Description("Donate your money towards the development of TBKBot")]
        public async Task DonateToBot(CommandContext ctx, [Description("Donate amount")] int amount = 0)
        {
            var db = new DBEngine();
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



        [Command("leaderboard"), Description("TBK money leaderboard")]
        [Aliases("lb")]
        public async Task MoneyLeaderboard(CommandContext ctx, int page = 1)
        {
            var db = new DBEngine();
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

        [Command("give"), Description("Give your money to a member")]
        public async Task GiveMoney(CommandContext ctx, DiscordMember member, int amount)
        {
            var db = new DBEngine();

            bool exchange = ctx.Member.Id == 425661467904180224 ? false : true;

            var benefactor = await db.LoadMemberAsync(ctx.Member.Id);
            var beneficiary = await db.LoadMemberAsync(member.Id);

            if (benefactor.Money < amount && exchange)
            {
                return;
            }

            benefactor.Money -= exchange ? amount : 0;
            beneficiary.Money += amount;

            await db.SaveMemberAsync(benefactor);
            await db.SaveMemberAsync(beneficiary);

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            await ctx.RespondAsync($"{amount:N0} {coinEmoji} has been given to {member.Username}. ({benefactor.Money:N0} {coinEmoji})");
        }

        [Command("reactpick"), Description("Pick a random member who reacted to a given emoji of a message. Reference (reply to) the message to register it.")]
        public async Task ReactionPick(CommandContext ctx, DiscordEmoji emoji)
        {
            // Get the message
            var message = ctx.Message.ReferencedMessage;
            if (message == null)
            {
                await ctx.RespondAsync("Message not found.");
                return;
            }

            var users = await message.GetReactionsAsync(emoji);

            if (users.Count == 0)
            {
                await ctx.RespondAsync("No users found who reacted with the randomly selected reaction.");
                return;
            }

            var randomUser = users[Rng.Next(users.Count)];

            await ctx.RespondAsync($"{randomUser.Mention}");
        }

        private Dictionary<ulong, string> cachedMessages = new Dictionary<ulong, string>();

        [Command("string"), Description("Generates a random message. Words are taken from last sent messages of the channel.")]
        public async Task RandomString(CommandContext ctx, [Description("Channel to randomize. Parameter defaults to current channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
            {
                channel = ctx.Channel;
            }

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

            int limit = Rng.Next(1, 20);

            var words = messageContents.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            words = [.. words.OrderBy(_ => Rng.Next())];

            List<string> randomWords = words.Take(limit).ToList();

            string result = string.Join(" ", randomWords);

            await ctx.RespondAsync(content: result.ToLower());
        }

        [Command("fiveoclock"), Description("It's five o'clock somewhere")]
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

        [Command("setbirthday"), Description("Set your birthday")]
        public async Task SetBirthday(CommandContext ctx, [RemainingText] string date)
        {
            var db = new DBEngine();

            DateTime dt;

            bool isValidFormat = DateTime.TryParseExact(date, "M/d", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                                 DateTime.TryParseExact(date, "M", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ||
                                 DateTime.TryParseExact(date, "MMM d", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt);

            if (!isValidFormat)
            {
                await ctx.RespondAsync("Failed to format input.");
                return;
            }

            dt.ToUniversalTime();

            var data = await db.LoadMemberAsync(ctx.User.Id);

            data.Birthday = dt;

            await db.SaveMemberAsync(data);

            await ctx.RespondAsync($"Your birthday has been saved as {dt:MMMM dd}");
        }

        [Command("speechbubble"), Description("Creates a speech bubble image.")]
        public async Task SpeechBubble(CommandContext ctx)
        {
            var referencedMsg = ctx.Message.ReferencedMessage;

            if (referencedMsg == null)
            {
                await ctx.RespondAsync("React to a message with an image attachment.");
                return;
            }

            if (referencedMsg.Attachments.Count == 0)
            {
                await ctx.RespondAsync("No image found.");
                return;
            }

            var url = referencedMsg.Attachments[0].Url;

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

        [Command("kill"), Description("Kill a member.")]
        public async Task Kill(CommandContext ctx, [Description("Member to kill")] DiscordMember member)
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

        [Command("deposit"), Description("Deposit money into your bank.")]
        [Aliases("dep")]
        public async Task Deposit(CommandContext ctx, [Description("Amount to depsit")] string amount)
        {
            var db = new DBEngine();

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

            var embed = new DiscordEmbedBuilder();

            embed.AddField("Wallet", $"{data.Money:N0} {coinEmoji}", true);
            embed.AddField("Bank", $"{data.Bank:N0} {coinEmoji}", true);

            await ctx.RespondAsync($"{amountInt:N0} {coinEmoji} deposited into the bank.", embed);
        }

        [Command("withdraw"), Description("Withdraw money from your bank.")]
        [Aliases("with")]
        public async Task Withdraw(CommandContext ctx, [Description("Amount to withdraw")] int amount)
        {
            var db = new DBEngine();

            var data = await db.LoadMemberAsync(ctx.User.Id);

            if (amount > data.Bank)
            {
                return;
            }

            data.Money += amount;
            data.Bank -= amount;

            await db.SaveMemberAsync(data);

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            var embed = new DiscordEmbedBuilder();

            embed.AddField("Wallet", $"{data.Money:N0} {coinEmoji}", true);
            embed.AddField("Bank", $"{data.Bank:N0} {coinEmoji}", true);

            await ctx.RespondAsync($"{amount:N0} {coinEmoji} withdrawn from the bank.", embed);
        }

        [Command("steal"), Description("Steal money from member.")]
        public async Task Steal(CommandContext ctx, [Description("Member to steal from")] DiscordMember member)
        {
            if (member == ctx.Member)
            {
                return;
            }

            var db = new DBEngine();

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
            

            int randomDivider = Rng.Next(10, 50);

            int amountStolen = victim.Money / randomDivider;

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

            var embed = new DiscordEmbedBuilder();

            embed.AddField(ctx.Member.DisplayName, $"{thief.Money:N0} {coinEmoji}", true);
            embed.AddField(member.DisplayName, $"{victim.Money:N0} {coinEmoji}", true);

            await ctx.RespondAsync($"Stole {amountStolen:N0} {coinEmoji} from {member.Mention}.", embed);
        }

        [Command("roulette"), Description("Play roulette.")]
        public async Task Roulette(CommandContext ctx, 
            [Description("Amount to bet")] int bet, 
            [Description("Roulette space to bet on (number, color)")] string space)
        {
            var db = new DBEngine();

            var data = await db.LoadMemberAsync(ctx.User.Id);

            bool success = data.RemoveMoney(bet);
            if (!success)
            {
                await ctx.RespondAsync("You don't have enough to bet.");
                return;
            }

            var coinEmoji = await ctx.Guild.GetEmojiAsync(1219476665852231751);

            var msg = await ctx.RespondAsync($"You have placed a bet of {bet} {coinEmoji} on `{space}`");

            string[] colors = ["red", "black"];

            string color = colors[Rng.Next(0, 2)];
            int number = Rng.Next(0, 36);

            int payout = 0;

            if (int.TryParse(space, out int spaceNumber))
            {
                if (spaceNumber == number)
                {
                    payout = 4;
                }
            }
            else if (colors.Contains(space))
            {
                if (space == color)
                {
                    payout = 2;
                }
            }
            else
            {
                await ctx.RespondAsync($"Invalid syntax: `{space}` cannot be used as a parameter.");
            }


            string messageText = $"The ball landed on: `{color} {number}`!";

            if (payout == 0)
            {
                await msg.ModifyAsync(messageText + $"\n\nYou did not win anything :(");
            }
            else
            {
                int amount = bet * payout;

                data.AddMoney(amount);

                await msg.ModifyAsync(messageText + $"\n\n**You won {amount} {coinEmoji}!**");
            }

            await db.SaveMemberAsync(data);
        }
    }
}