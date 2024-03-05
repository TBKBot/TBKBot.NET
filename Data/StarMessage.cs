using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TBKBot.Models
{
    internal class StarMessage
    {
        public ulong Id { get; set; }
        public ulong ChannelId { get; set; }
        public ulong AuthorId { get; set; }
        public string Content { get; set; }
        public Uri JumpLink { get; set; }
        public ulong BoardId { get; set; }
        public int Stars { get; set; }

        private static readonly string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Starboards");
        private static readonly ulong starboardId = 737683342371061850;

        public StarMessage(DiscordMessage message)
        {
            Id = message.Id;
            ChannelId = message.ChannelId;
            AuthorId = message.Author.Id;
            Content = message.Content;
            JumpLink = message.JumpLink;
        }

        public async Task SaveAsync()
        {
            string filePath = Path.Combine(directoryPath, $"{Id}.json");
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            await File.WriteAllTextAsync(filePath, json);
        }

        public static async Task<StarMessage> LoadAsync(DiscordMessage message)
        {
            string filePath = Path.Combine(directoryPath, $"{message.Id}.json");

            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);
                var starMessage = JsonConvert.DeserializeObject<StarMessage>(json);
                await UpdateStarboardMessage(starMessage, message);
                return starMessage;
            }
            else
            {
                await CreateStarboardMessage(message);
                return new StarMessage(message);
            }
        }

        private static async Task CreateStarboardMessage(DiscordMessage message)
        {
            var author = await Program.Client.GetUserAsync(message.Author.Id);

            int starCount = message.Reactions.FirstOrDefault(x => x.Emoji.Name == "⭐").Count;

            var boardChannel = message.Channel.Guild.GetChannel(starboardId);
            if (boardChannel == null)
            {
                Console.WriteLine("Starboard channel not found.");
                return;
            }

            var boardMsg = await boardChannel.SendMessageAsync($"**{author.Username}** said: \"{message.Content}\" has reached {starCount} ⭐");

            var starMessage = new StarMessage(message)
            {
                BoardId = boardMsg.Id
            };

            await starMessage.SaveAsync();
        }

        private static async Task UpdateStarboardMessage(StarMessage starMessage, DiscordMessage message)
        {
            var boardChannel = message.Channel.Guild.GetChannel(starboardId);
            if (boardChannel == null)
            {
                Console.WriteLine("Starboard channel not found.");
                return;
            }

            var boardMsg = await boardChannel.GetMessageAsync(starMessage.BoardId);
            if (boardMsg == null)
            {
                Console.WriteLine("Starboard message not found.");
                return;
            }

            int starCount = message.Reactions.FirstOrDefault(x => x.Emoji.Name == "⭐").Count;

            await boardMsg.ModifyAsync($"**{message.Author.Username}** said: \"{message.Content}\" has reached {starCount} ⭐");
        }
    }
}
