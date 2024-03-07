using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Globalization;
using System.Text.RegularExpressions;
using TBKBot.Data;

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
                await ctx.RespondAsync("RoleHierarchyError: Provided member role level is too high");
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
        public async Task Coin(CommandContext ctx)
        {
            var message = await ctx.RespondAsync("Throwing coin...");

            Random rnd = new Random();

            var index = rnd.Next(0, 1);

            if (index == 0)
            {
                await message.ModifyAsync("The coin landed on **heads**!");
            }
            else if (index == 1)
            {
                await message.ModifyAsync("The coin landed on **tails**!");
            }
        }

        /*
        [Command("play")]
        [Aliases("p")]
        public async Task PlayMusic(CommandContext ctx, [Description("Query search term on YouTube")][RemainingText] string query)
        {
            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            //PRE-EXECUTION CHECKS
            if (ctx.Member.VoiceState == null || userVC == null)
            {
                await ctx.RespondAsync("Please enter a voice channel.");
                return;
            }

            if (!lavalinkInstance.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Connection is not establihsed: no Lavalink nodes are connected.");
                return;
            }

            if (userVC.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Invalid voice channel. Please enter a valid channel.");
                return;
            }

            if (query == null)
            {
                await ctx.RespondAsync("Please provide a search query.");
                return;
            }

            //Connecting to VC and playing music
            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink failed to connect.");
                return;
            }

            var searchQuery = await node.Rest.GetTracksAsync(query);
            if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await ctx.RespondAsync($"Failed to find music with query: {query}");
                return;
            }

            var musicTrack = searchQuery.Tracks.First();

            await conn.PlayAsync(musicTrack);

            var converter = new Utils.ConvertText();

            string musicDescription = $"Now Playing: [{musicTrack.Title}]({musicTrack.Uri})\nChannel: {musicTrack.Author}\nDuration: {converter.TimeFormat(musicTrack.Length)}";
            string id = musicTrack.Identifier;
            var nowPlayingEmbed = new DiscordEmbedBuilder()
            {
                Color = Program.EmbedColor,
                Title = $"Successfully joined {userVC.Name}",
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = $"https://img.youtube.com/vi/{id}/hqdefault.jpg"
                },
                Description = musicDescription
            };

            await ctx.RespondAsync(embed: nowPlayingEmbed);
        }

        [Command("pause")]
        public async Task PauseMusic(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks playing.");
                return;
            }

            await conn.PauseAsync();
        }

        [Command("resume")]
        public async Task ResumeMusic(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks playing.");
                return;
            }

            await conn.ResumeAsync();
        }

        [Command("seek")]
        public async Task SeekMusic(CommandContext ctx, int seekValue)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks playing.");
                return;
            }

            var position = TimeSpan.FromSeconds(seekValue);

            await conn.SeekAsync(position);

            await ctx.RespondAsync($"Seeked track to {position}");
        }

        [Command("nowplaying")]
        [Aliases("np")]
        public async Task NowPlaying(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks playing.");
                return;
            }

            var currentTrack = conn.CurrentState.CurrentTrack;
            var position = conn.CurrentState.PlaybackPosition;
            var length = currentTrack.Length;

            string id = conn.CurrentState.CurrentTrack.Identifier;

            var converter = new Utils.ConvertText();

            var nowPlayingEmbed = new DiscordEmbedBuilder()
            {
                Title = "Now Playing",
                Color = Program.EmbedColor,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = $"https://img.youtube.com/vi/{id}/hqdefault.jpg"
                },
                Description = $"[{currentTrack.Title}]({currentTrack.Uri}) by {currentTrack.Author}\n{converter.TimeFormat(position)} {GenerateProgressBar(position, length)} {converter.TimeFormat(length)}"
            };

            await ctx.RespondAsync(nowPlayingEmbed);
        }

        [Command("leave")]
        public async Task LeaveVC(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks playing.");
                return;
            }

            await conn.DisconnectAsync();

            await ctx.RespondAsync("Bot left voice channel.");
        }
        */

        [Group("poll")]
        class Polls
        {
            [GroupCommand]
            [Command("create")]
            public async Task PollCreate(CommandContext ctx)
            {
                await ctx.RespondAsync("Command is in the works.");
            }
        }

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

            var DBEngine = new DBEngine("tbkbot");
            var data = await DBEngine.LoadMemberAsync(member.Id);

            if (data == null)
            {
                await ctx.RespondAsync("Something wrong occured: Failed to retrieve member data");
                return;
            }

            await ctx.RespondAsync($"{member.Username}'s balance: ${data.Money.ToString("N0")}");
        }


        [Command("donate")]
        public async Task DonateToBot(CommandContext ctx, int amount = 0)
        {
            var DBEngine = new DBEngine("tbkbot");
            var data = await DBEngine.LoadMemberAsync(ctx.User.Id);

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

            await DBEngine.SaveMemberAsync(data);
        }



        [Command("leaderboard")]
        [Aliases("lb")]
        public async Task MoneyLeaderboard(CommandContext ctx)
        {
            var DBEngine = new DBEngine("tbkbot");

            var members = await DBEngine.GetAllMembersAsync();

            var memberDict = new Dictionary<string, int>();

            foreach (var member in members)
            {
                string idString = member.Id.ToString();
                memberDict.Add(idString, member.Money);
            }

            // Sort the dictionary by value in descending order
            var sortedMembers = memberDict.OrderByDescending(pair => pair.Value);

            var embed = new DiscordEmbedBuilder
            {
                Title = "Money Leaderboard",
                Color = new DiscordColor("#FFD700") // Set your desired color
            };

            int i = 1;
            foreach (var pair in sortedMembers.Take(10)) // Take only the top 10 members
            {
                embed.Description += $"{i} - <@{pair.Key}> - ${pair.Value:N0}\n";
                i++;
            }

            // Send the leaderboard embed
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("give")]
        public async Task GiveMoney(CommandContext ctx, DiscordMember member, int amount)
        {
            var DBEngine = new DBEngine("tbkbot");

            bool exchange = true;

            if (ctx.Member.Id == 425661467904180224)
            {
                exchange = false;
            }

            if (exchange)
            {
                var benefactor = await DBEngine.LoadMemberAsync(ctx.Member.Id);

                if (benefactor.Money < amount)
                {
                    amount = benefactor.Money;
                }
                benefactor.Money -= amount;

                await DBEngine.SaveMemberAsync(benefactor);
            }

            var beneficiary = await DBEngine.LoadMemberAsync(member.Id);

            beneficiary.Money += amount;

            await DBEngine.SaveMemberAsync(beneficiary);

            await ctx.RespondAsync($"${amount:N0} has been given to {member.Username}.");
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

            var message = await ctx.RespondAsync("Generating...");

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

            // Regular expression pattern to match URLs
            string pattern = @"(?:https?|ftp):\/\/[\n\S]+";

            // Remove URLs from the concatenated string
            messageContents = Regex.Replace(messageContents, pattern, "");

            // Split the concatenated string into words
            var words = messageContents.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Shuffle the words to get a random order
            words = words.OrderBy(_ => random.Next()).ToArray();

            // Pick random words from the shuffled list
            List<string> randomWords = words.Take(limit).ToList();

            // Join the random words into a string
            string result = string.Join(" ", randomWords);

            // Send the string as a message
            await message.ModifyAsync(content: result.ToLower());
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

                    string region = displayNameParts[1].Trim();
                    string timeZoneId = displayNameParts[0].Trim();

                    await ctx.RespondAsync($"It's 5 o' clock in: [{region}](<https://www.google.com/search?q={region}>) ({timeZoneId})");
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

                var DBEngine = new DBEngine("tbkbot");

                var data = await DBEngine.LoadMemberAsync(ctx.User.Id);

                data.Birthday = dt;

                await DBEngine.SaveMemberAsync(data);

                await ctx.RespondAsync($"Your birthday has been saved to {dt:MMMM dd}");
            }
            catch (Exception ex)
            {
                await ctx.RespondAsync(ex.Message);
            }
        }

        private static string GenerateProgressBar(TimeSpan currentPosition, TimeSpan trackLength)
        {
            const int progressBarLength = 20;

            // calculate the ratio of completed time to total time
            double progressRatio = currentPosition.TotalSeconds / trackLength.TotalSeconds;

            // calculate the number of filled and empty slots in the progress bar
            int filledSlots = (int)(progressBarLength * progressRatio);
            int emptySlots = progressBarLength - filledSlots;

            // generate the progress bar string
            string progressBar = new string('━', filledSlots);
            progressBar += "⬤";
            progressBar += new string('─', emptySlots);

            return progressBar;
        }
    }
}