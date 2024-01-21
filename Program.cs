using DSharpPlus;
using DSharpPlus.CommandsNext;
using TBKBot.commands;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using System;
using DSharpPlus.Entities;
using System.Linq;
using TBKBot.other;

namespace TBKBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        public static DiscordMessage snipeMsg { get; set; }
        static async Task Main(string[] args)
        {
            var jsonReader = new JSONReader();
            await jsonReader.ReadJSON();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true

            };

            Client = new DiscordClient(discordConfig);

            var slash = Client.UseSlashCommands();

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            Client.Ready += Client_Ready;

            var logger = new Logging();

            Client.MessageCreated += async (s, e) =>
            {
                if (e.Message.Author != Client.CurrentUser && e.Message.Content.ToLower().Contains("balls"))
                    await e.Message.RespondAsync("balls");
            };

            Client.MessageDeleted += async (s, e) =>
            {
                await logger.LogDeletion(e.Message);

                snipeMsg = e.Message; // saves the deleted message for snipe command use
                await Task.Delay(60000);
                snipeMsg = null;
            };
            
            Client.MessageUpdated += async (s, e) =>
            {
                if (!e.Message.Author.IsBot)
                {
                    if (e.MessageBefore.Content != e.Message.Content)
                    {
                        await logger.LogEdit(e.MessageBefore, e.Message);
                    }
                }
            };

            Client.MessageReactionAdded += async (s, e) =>
            {
                var reaction = await e.Message.GetReactionsAsync(e.Emoji);
            };

            Client.MessageReactionRemoved += async (s, e) =>
            {
                var reaction = await e.Message.GetReactionsAsync(e.Emoji);
            };

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<Commands>();

            await Client.ConnectAsync();

            Console.WriteLine($"Connected user:\n{Client.CurrentUser.Username}\nID: {Client.CurrentUser.Id}\n");


            await Task.Delay(-1);
        }

        private static Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
