using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;
using TBKBot.Data;
using TBKBot.Models;

public class MessageCreationHandler
{
    private readonly DiscordClient _client;

    public MessageCreationHandler(DiscordClient client)
    {
        _client = client;
        _client.MessageCreated += OnMessageCreated;
    }

    public async Task OnMessageCreated(DiscordClient s, MessageCreateEventArgs e)
    {
        double chance = new Random().NextDouble();

        // invalidate event from this bot's user
        if (e.Author == s.CurrentUser)
        {
            return;
        }

        // get suggestions
        if (e.Channel.Id == 704969433092849665 || e.Channel.Id == 745094818547630080)
        {
            if (e.Author.IsBot)
                return;

            var suggestionLogChannel = e.Guild.GetChannel(819408070437502976);

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"Suggestion from #{e.Channel.Name}",
                Description = e.Message.Content,
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = e.Author.Username,
                    IconUrl = e.Author.AvatarUrl,
                    Url = e.Message.JumpLink.ToString(),
                },
                Timestamp = DateTime.UtcNow
            };

            var builder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(new DiscordComponent[]
                {
                    new DiscordButtonComponent(ButtonStyle.Success, "suggestion_yes", "Staff", emoji: new DiscordComponentEmoji(774965290957406259)),
                    new DiscordButtonComponent(ButtonStyle.Danger, "suggestion_no", "Delete", emoji: new DiscordComponentEmoji(774965217376731147)),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "suggestion_staff", "Guards", emoji: new DiscordComponentEmoji(789569436302049280)),
                    new DiscordButtonComponent(ButtonStyle.Primary, "suggestion_public", "Public", emoji: new DiscordComponentEmoji(774965329674895370))
                });
                
            await suggestionLogChannel.SendMessageAsync(builder);
        }


        // balls
        if (e.Message.Content.ToLower().Contains("balls"))
        {
            var lastMessages = await e.Channel.GetMessagesAsync(100);

            string pattern = @"(balls)";

            DiscordMessage msg;

            MatchCollection matches = Regex.Matches(e.Message.Content, pattern, RegexOptions.IgnoreCase);

            await e.Channel.TriggerTypingAsync();

            if (matches.Count > 1 && chance < 0.2)
            {
                msg = await e.Message.RespondAsync("you sure like balls, huh?");
            }
            else if (lastMessages.Count(x => x.Content.ToLower().Contains("balls")) > 50)
            {
                List<string> messages = new List<string>() { "balls", "why are you like this?", "dont you have anything better to do?", "you need help", "bruh", "balls balls balls balls", ":pensive:" };

                int randIdx = new Random().Next(0, messages.Count);

                msg = await e.Message.RespondAsync(messages[randIdx]);
            }
            else
            {
                msg = await e.Message.RespondAsync("balls");

            }

            if (chance < 0.01)
            {
                await msg.CreateReactionAsync(DiscordEmoji.FromUnicode("🎊"));
            }
        }


        // i love gd cologne
        if (e.Message.Content.ToLower().Contains("sightread"))
        {
            var sightreadableEmbed = new DiscordEmbedBuilder
            {
                Description = "⬛\U0001f7e7\U0001f7e7\U0001f7e7\U0001f7e7\U0001f7e7\U0001f7e7⬛⬛\r\n⬛\U0001f7e7⬛⬛⬛⬇️\U0001f7e7⬛⬛\r\n⬛\U0001f7e7⬛⬛⬛⬛\U0001f7e7⬛⬛\r\n⬛⬛➡️⬛⬛⬛\U0001f7e7⬛⬛\r\n\U0001f7e7\U0001f7e7\U0001f7e7\U0001f7e7🔺🔺\U0001f7e7⬛⬛\r\n⬛⬛⬛\U0001f7e7⬛⬛⬛⬛⬛\r\n⬛⬛⬛\U0001f7e7\U0001f7e7\U0001f7e7\U0001f7e7\U0001f7e7\U0001f7e7"
            };

            await e.Message.RespondAsync(sightreadableEmbed);
        }


        // portuguese
        string[] portugese = { "portugal", "portuguese", "🇵🇹" }; // matches every case in array

        foreach (var port in portugese)
        {
            if (e.Message.Content.ToLower().Contains(port))
            {
                if (chance > 0.01)
                {
                    await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/934791535902470167/1205833685635440660/youre_so_portuguese.mp4?ex=65d9cf21&is=65c75a21&hm=7192ddae25319a289d436befa5cb07f56e7fbb123994e62dc57b6c8437f69585&");
                }
                else // optic cable core gacha
                {
                    await e.Message.RespondAsync("https://cdn.discordapp.com/attachments/1043377446805835827/1217565766061134045/portuguese.mp4?ex=66047d7b&is=65f2087b&hm=895b0c4ec99617c035b9d0ed9cfb444ddf10eee4612cdad9ef3eecba9b52ca22&");
                }
            }
        }

        // lobotomy
        if (e.Message.Content.ToLower().Contains("lobotom"))
            await e.Message.RespondAsync("https://cdn.discordapp.com/emojis/1216109488294068314.webp?size=160&quality=lossless");


        // increment member data
        if (e.Author.IsBot)
            return;

        var db = new DBEngine("tbkbot");

        var member_data = await db.LoadMemberAsync(e.Message.Author.Id);

        if (member_data == null)
        {
            member_data = new GuildMember
            {
                Id = e.Message.Author.Id,
                Username = e.Message.Author.Username,
                Money = 0,
                Bank = 0,
                Birthday = null
            };
        }

        member_data.Money++;

        await db.SaveMemberAsync(member_data);
    }
}