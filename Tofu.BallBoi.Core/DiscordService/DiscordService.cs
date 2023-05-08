
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using System.Security.Cryptography;
using System.Text;

namespace Tofu.BallBoi.Core.DiscordService
{
    public class DiscordService
    {
        private readonly ILogger<DiscordService> _logger;
        private readonly DiscordSocketClient _discordSocketClient;
        private readonly InteractionService _interactionService;
        public DiscordService(ILogger<DiscordService> logger)
        {
            _logger = logger;
            _discordSocketClient = new();
            _interactionService = new(_discordSocketClient);
        }

        private void InitializeDiscordClient()
        {
            _logger.LogInformation("Initializing Discord Client");
            _discordSocketClient.Ready += HandleClientReadyAsync;
            _discordSocketClient.SlashCommandExecuted += SlashCommandhandler;
            _discordSocketClient.ApplicationCommandCreated += HandleApplicationCommandCreated;
            _discordSocketClient.ApplicationCommandDeleted += HandleApplicationCommandDeleted;
            _discordSocketClient.ApplicationCommandUpdated += HandleApplicationCommandUpdated;

        }

        


        private async Task HandleClientReadyAsync()
        {
            _logger.LogInformation("Discord Client Ready");

        }

        private async Task SlashCommandhandler(SocketSlashCommand cmd)
        {
            try
            {
                switch (cmd.CommandName)
                {
                    case "remember": RememberHandlerAsync(cmd); break;
                    case "version": throw new NotImplementedException(); break;
                    case "addrole": throw new NotImplementedException(); break;
                    case "removerole": throw new NotImplementedException(); break;
                    case "listrole": throw new NotImplementedException(); break;
                    case "chat": ChatAsync(cmd); break;
                    case "checkavailabletokens": throw new NotImplementedException(); break;
                    case "checkmessagetoken": throw new NotImplementedException(); break;
                    default:
                        Console.WriteLine("Unknown Command recieved in SlashCommandhandler");
                        break;
                }
                //await command.RespondAsync($"You executed {command.Data.Name}");
            }
            catch (NotImplementedException e)
            {
                await cmd.RespondAsync("That command isn't ready yet");
                _logger.LogWarning($"A command {cmd.CommandName} was called that is not implemented");
            }
        }

        private async Task HandleApplicationCommandCreated(SocketApplicationCommand cmd)
        {
            _logger.LogInformation($"Application Command Created {cmd.Name}");
        }
        private async Task HandleApplicationCommandDeleted(SocketApplicationCommand cmd)
        {
            _logger.LogInformation($"Application Command Deleted {cmd.Name}");
        }
        private async Task HandleApplicationCommandUpdated(SocketApplicationCommand cmd)
        {
            _logger.LogInformation($"Application Command Updated {cmd.Name}");
        }
        private void GetGlobalRegisteredCommands()
        {

        }
        private void GetGuildRegisteredCommands()
        {

        }
        private void RegisterCommands()
        {

        }
        private async void RememberHandlerAsync(SocketSlashCommand cmd)
        {
            await cmd.RespondAsync($"https://cdn.discordapp.com/attachments/936034644166598760/945888996356149288/image0.jpg");
        }
        private async void AddRoleHandlerAsync()
        {

        }
        private async void ChatAsync(SocketSlashCommand command)
        {

            //var user = GetUserFromCommand(command);
            //Console.WriteLine(user.Id);
            //Console.WriteLine(user.DisplayName);
            var warnAknowledged = false;
            var tokensAvailable = false;

            //the user management should be split into a seperate microservice

            command.RespondAsync($"Processing prompt");
            try
            {
                Console.WriteLine("Received chat prompt");

                dbHelper.CreateUserTableIfNotExists(_connection);
                dbHelper.CreatePromptTableIfNotExists(_connection);
                dbHelper.CreateResponseTableIfNotExists(_connection);
                var username = command.User.Username;
                if (!dbHelper.UserExistsInDB(_connection, command.User.Id))
                {

                    Console.WriteLine("User is not registered yet");
                    int result = dbHelper.AddNewUserToUsersTable(_connection, command.User.Id, command.User.Username);
                    Console.WriteLine($"Added User to DB, Result: {result}");
                    //ChatWarn(command);
                    //await command.Channel.SendMessageAsync("Hello, You will be awarded 5000 tokens and issued 1 tokens per 208 seconds back up to the 5000 token limit. This api costs money so play fair please.");

                }

                var user = dbHelper.GetUserFromUsersTable(_connection, command.User.Id);
                string prompt = command.Data.Options.ElementAt(0).Value.ToString();

                Console.WriteLine($"User: {username} | prompt: {prompt}");

                var tokens = GPT3Tokenizer.Encode(prompt);
                user = dbHelper.GetUpdatedUser(_connection, user);
                if (user.AvailableTokens > tokens.Count())
                {
                    Console.WriteLine("user has enough tokens");
                    dbHelper.UpdateUserTokens(_connection, user, tokens.Count());

                    var chat = _openAI.Chat.CreateConversation();
                    chat.AppendUserInput(prompt);

                    var channel = command.Channel;
                    var response = new StringBuilder();

                    response.Append($"{command.User.Username} asked: {prompt}\n\n");

                    //iterate over the streaming response from the api and send it out in discord messages.
                    //discord has a 2000 character limit so we need to split long messages into groups.
                    int nextTarget = 0;
                    int curMsgLength = 0;
                    int curMsg = 0;
                    RestUserMessage newMsg = null;
                    await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
                    {
                        curMsgLength += res.Length;
                        response.Append(res.ToString());
                        Console.WriteLine(response.Length);
                        if (curMsgLength > 1900)
                        {
                            response = new StringBuilder();
                            response.Append(res.ToString());
                            curMsg++;
                            curMsgLength = 0;
                            await command.Channel.SendMessageAsync("...");
                            newMsg = await command.Channel.SendMessageAsync(response.ToString());
                        }
                        if (curMsg == 0)
                        {
                            if (response.Length > nextTarget)
                            {
                                nextTarget += 25;
                                await command.ModifyOriginalResponseAsync(msg => msg.Content = response.ToString());
                            }
                        }
                        else
                        {
                            await newMsg.ModifyAsync(msg => msg.Content = response.ToString());

                        }
                    }
                    await command.ModifyOriginalResponseAsync(msg => msg.Content = response.ToString());
                    Console.WriteLine("done processing");
                    //dbHelper.InsertPrompt(_connection, user, tokens.Count(), prompt);
                    //var responseTokens = GPT3Tokenizer.Encode(prompt);
                    //dbHelper.InsertResponse(_connection, user, responseTokens.Count(), response.ToString());
                }
                else
                {
                    Console.WriteLine("User does not have enough tokens");
                    //not enough tokens
                    await command.Channel.SendMessageAsync("Sorry, You don't have enough tokens, please try again later.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await command.Channel.SendMessageAsync("an error occurred, try again");
                await command.Channel.SendMessageAsync($"https://cdn.discordapp.com/attachments/936034644166598760/945888996356149288/image0.jpg");

            }
        }



    }
}
