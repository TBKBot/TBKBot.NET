using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TBKBot.Commands;
using TBKBot.Commands.Slash;
using TBKBot.Data;
using TBKBot.Events;

namespace TBKBot
{
    internal class Program
    {
        public static DiscordClient Client { get; set; }
        private static CommandsNextExtension Commands { get; set; }
        public static DBEngine DBEngine { get; set; }

        public static MessageDeletionHandler _messageDeletionHandler;
        public static MessageUpdateHandler _messageUpdateHandler;

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

            //Initialize global DB instance
            DBEngine = new(jsonReader.MongoUrl);

            //Set default timeout for interactivity commands
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            //Event Handling
            var messageCreationHandler = new MessageCreationHandler(Client);
            _messageUpdateHandler = new MessageUpdateHandler(Client);
            _messageDeletionHandler = new MessageDeletionHandler(Client);
            var memberJoinHandler = new MemberJoinHandler(Client);
            var memberLeaveHandler = new MemberLeaveHandler(Client);
            var reactAddHandler = new ReactionAddHandler(Client);
            var componentInteractHandler = new ComponentInteractionHandler(Client);


            var services = new ServiceCollection()
                .AddSingleton<Random>()
                .BuildServiceProvider();

            //Setting up command configuration
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { jsonReader.Prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = true,
                Services = services
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
