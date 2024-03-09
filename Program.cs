using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using TBKBot.commands;
using TBKBot.Commands.Slash;
using TBKBot.Events;

namespace TBKBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        public static DiscordColor EmbedColor = new DiscordColor(64, 12, 188);

        public static MessageDeletionHandler _messageDeletionHandler;

        static async Task Main(string[] args)
        {
            // Deserialize config.json
            var jsonReader = new JSONReader();
            await jsonReader.ReadJSON();

            //Setting up the bot configuration
            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = jsonReader.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug

            };

            //Apply config to discord bot
            Client = new DiscordClient(discordConfig);

            //Set default timeout for interactivity commands
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            //Event Handling
            var messageCreationHandler = new MessageCreationHandler(Client);
            var messageUpdateHandler = new MessageUpdateHandler(Client);
            _messageDeletionHandler = new MessageDeletionHandler(Client);
            var memberJoinHandler = new MemberJoinHandler(Client);
            var memberLeaveHandler = new MemberLeaveHandler(Client);
            var reactAddHandler = new ReactionAddHandler(Client);
            var componentInteractHandler = new ComponentInteractionHandler(Client);


            //Setting up command configuration
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            var slashCommandsConfiguration = Client.UseSlashCommands();

            //Registering commands
            Commands.RegisterCommands<PrefixCommands>();
            slashCommandsConfiguration.RegisterCommands<SlashCommands>();


            Client.Ready += Client_Ready;

            //Connect the bot
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
