using AI.Dev.OpenAI.GPT;
using BallBoi;
using Core;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Text;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Text.RegularExpressions;
using Google.Apis.YouTube.v3.Data;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.ComponentModel;
using Newtonsoft.Json.Converters;
using OpenAI_API.Moderation;
using System.Linq;
using System.Data.Common;

//Things the bot can do: Ideas
//music playlist, remove the embed, after x time, rewrite it to a regular
public class Program
{
    private VersionReporter _versionReporter = new();

    private DiscordSocketClient _client;
    private ulong guildId;
    private SocketGuild _guild;
    private OpenAI_API.OpenAIAPI _openAI;
    SqliteConnection _connection;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        using FileStream fileStream = File.Open("users.db", FileMode.Append);
        fileStream.Close();
        _connection = new SqliteConnection("data Source=users.db");
        Console.WriteLine($" Version: {_versionReporter.CurrentVersion.FullVersionNumberString}");
        if (_versionReporter.IsNewerVersionAvailable())
        {
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine("New Version Available");
            Console.WriteLine($"{_versionReporter.NextVersion.FullVersionNumberString}");
            Console.WriteLine($"Release Type: {_versionReporter.NextVersionType}");
            Console.WriteLine($"Release Date: {_versionReporter.NextVersionDate}");
            Console.WriteLine("------------------------------------------------------");
        }


        var apiKey = Environment.GetEnvironmentVariable("APIKEY");
        //guildId = ulong.Parse(Environment.GetEnvironmentVariable("GUILDID"));

        Console.WriteLine(apiKey);

        var gptKey = Environment.GetEnvironmentVariable("OPENAIKEY");
        _openAI = new OpenAI_API.OpenAIAPI(gptKey);


        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;
        _client.ModalSubmitted += HandleModalSubmitted;
        //_client.ButtonExecuted += HandleButtonExecuted;
        _client.InteractionCreated += HandleInteractionCreated;

        // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
        // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
        // var token = File.ReadAllText("token.txt");
        // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

        var _interactionService = new InteractionService(_client.Rest);

        if (apiKey != "key")
        {
            await _client.LoginAsync(TokenType.Bot, apiKey);
            await _client.StartAsync();
        }
        else
        {
            Console.WriteLine("Apikey not found");
            Console.WriteLine("Please set the key in envuronment variables and restart the application");
        }

        _guild = _client.GetGuild(guildId);



        async Task ClientReady()
        {
            //TODO: change all of this to just check if the command exists and build it if it doesn't.
            //this way new commands will automatically be deployed to the server on update.

            RegisterCommands();

        }

        //208 magic number is 5k tokens per 24 hours
        //incrementing so people feel like they are earning them over time
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(208));

        while (await timer.WaitForNextTickAsync())
        {
            //RewardTokens(_connection);
        }


        //ChatAsync();
        //Block until the application is closed.
        Console.WriteLine("Ready");
        await Task.Delay(-1);
    }

    private void OverWriteAllGlobalCommands()
    {
        //_client.BulkOverwriteGlobalApplicationCommandsAsync();
    }


    private async Task HandleInteractionCreated(SocketInteraction interaction)
    {

        Console.WriteLine("interactioncreated");
        if (interaction is SocketMessageComponent component && component.Data.Type == ComponentType.Button)
        {
            
            var embed = component.Message.Embeds.First();
            var footer = embed.Footer.HasValue ? long.Parse(embed.Footer.Value.Text) : -1;

            await component.RespondAsync($"{dbHelper.GetMessage(_connection, footer)}", ephemeral: true);
        }


    }
    //private async Task HandleButtonExecuted(SocketMessageComponent component)
    //{
        
    //    var a = component.Message.Content;
    //    var b = component.Data.Value;

    //    await component.RespondAsync("hello", ephemeral: true);

    //    var c = 

    //}

    private async Task HandleModalSubmitted(SocketModal modal)
    {
        //abstract this out to a switch handler like the slash commands

        Console.WriteLine("deferring response");
        await modal.DeferAsync();
        Console.WriteLine("Entering Typing State");
        var typingState = modal.Channel.EnterTypingState();
        List<SocketMessageComponentData> components = modal.Data.Components.ToList();
        var prompt = components.First(x => x.CustomId == "user_prompt").Value;
        //var parsed = double.TryParse(components.First(x => x.CustomId == "temp").Value, out double temp);
        double result = double.TryParse(components.First(x => x.CustomId == "temp").Value, out double temp) && temp >= 0 && temp <= 1 ? temp : 1;

        Console.WriteLine("Generating Chat response");
        var chatMessage = await CreateChatResponse(prompt, result);
        var embed = new EmbedBuilder();
        var button = new ComponentBuilder().WithButton("See Full Message", "see_full_message", style: ButtonStyle.Secondary);
        var messageId = await dbHelper.InsertMessage(_connection, chatMessage);


        var mention = new AllowedMentions();
        mention.AllowedTypes = AllowedMentionTypes.Users;


        var messageStart = chatMessage.AsSpan(0, Math.Min(500, chatMessage.Length)).ToString() + "...";
        
        embed.WithTitle("BallBoi Chat").WithColor(Color.Blue);
        embed.AddField($"{modal.User.Username}'s Prompt", prompt);
        embed.AddField("My response", messageStart).WithCurrentTimestamp().WithFooter(footer => footer.Text = $"{messageId}");
        Console.WriteLine("Sending Message");
        await modal.Channel.SendMessageAsync(embed: embed.Build(), components: button.Build());
        typingState.Dispose();
    }



    private async Task<string> CreateChatResponse(string prompt, double temp)
    {
        //var result = await _openAI.Completions.GetCompletion(prompt);

        var request = new OpenAI_API.Completions.CompletionRequest();
        request.Prompt = prompt;
        request.MaxTokens = 250;
        request.Temperature = temp;
        Console.WriteLine("Getting text completion " + DateTime.UtcNow);
        var result = await _openAI.Completions.CreateCompletionAsync(request);
        Console.WriteLine("Received text completion " + DateTime.UtcNow);
        return result.Completions[0].Text;
        //var chat = _openAI.Chat.CreateConversation();
        //chat.AppendUserInput(prompt);
        //Console.WriteLine();
        //var messageStr = new StringBuilder();
        //await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
        //{
        //    Console.Write(res);
        //    messageStr.Append(res);
        //}
        //return messageStr.ToString();
    }
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        try
        {
            switch (command.CommandName)
            {
                case "remember": Remember(command); break;
                case "version": Version(command); break;
                case "addrole": AddRole(command); break;
                case "removerole": throw new NotImplementedException(); break;
                case "listrole": ListRoles(command); break;
                case "chat": HandleChatTwoAsync(command); break;
                case "checkavailabletokens": UserTokenCheck(command); break;
                case "checkmessagetoken": PromptTokenCheck(command); break;
                case "video": HandleVideoCommand(command); break;
                default:
                    Console.WriteLine("Unknown Command recieved in SlashCommandhandler");
                    break;
            }
            //await command.RespondAsync($"You executed {command.Data.Name}");
        }
        catch (NotImplementedException e)
        {
            await command.RespondAsync("That command isn't ready yet");
        }


    }

    //private async Task SendAuditMessage()
    //{
    //    var embed = new EmbedBuilder().WithTitle("Test Audit Message");
    //    embed.Build();
    //    var channel = _client.GetChannel(1110743242749771826);
    //    var chanType = channel.GetChannelType();
        
    //}

    private async Task RegisterCommands()
    {

        // the solution here is to get all of the commands, match them and either remove or add them based on the listing here.
        //this can happen on the client ready and removes the requirement of setting the build flag or not
        try
        {
            var globalCommands = await _client.GetGlobalApplicationCommandsAsync();
            //var slashCommands = await _client.Applica
            var slashCommandList = new List<SlashCommandBuilder>()
            {
                new SlashCommandBuilder().WithName("remember").WithDescription("Most accidents happen at home"),
                new SlashCommandBuilder().WithName("version").WithDescription("Display version/about information"),
                new SlashCommandBuilder().WithName("listrole").WithDescription("List the roles a user has").AddOption("user", ApplicationCommandOptionType.User, "The user whose Roles you want to list", isRequired: true),
                new SlashCommandBuilder().WithName("chat").WithDescription("Ask Ballboi a question"),
                new SlashCommandBuilder().WithName("checkavailabletokens").WithDescription("Check how many tokens are in your account"),
                new SlashCommandBuilder().WithName("checkmessagetoken").WithDescription("Check how many tokens a given message is"),
                new SlashCommandBuilder().WithName("video").WithDescription("link a video to the videos channel").AddOption("url", ApplicationCommandOptionType.String,"the link to the video", isRequired: true),
            };
            foreach (var slashCommand in slashCommandList)
            {
                var commandExists = globalCommands.Select(x => x.Name == slashCommand.Name).Any();
                //if (commandExists) 
                //{
                //    Console.WriteLine($"Command {slashCommand.Name} already registered, skipping...");
                //}
                //else
                //{
                //    Console.WriteLine($"Adding Slash Command (Global) {slashCommand.Name}");
                //    await _client.CreateGlobalApplicationCommandAsync(slashCommand.Build());
                //}
                Console.WriteLine($"Adding Slash Command (Global) {slashCommand.Name}");
                await _client.CreateGlobalApplicationCommandAsync(slashCommand.Build());

            }

            var messageCommandList = new List<MessageCommandBuilder>() { };

            foreach (var messageCommand in messageCommandList)
            {
                await _client.CreateGlobalApplicationCommandAsync(messageCommand.Build());
            }

        }
        catch (ApplicationCommandException exception)
        {
            Console.WriteLine(exception.Message);
            // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            //TODO: write to error log
            Console.WriteLine(json);
        }
    }
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    #region SlashCommands
    //These Should be event based maybe



    private async void HandleChatTwoAsync(SocketSlashCommand cmd)
    {
        try
        {
            var modal = new ModalBuilder()
                        .WithTitle("Chat Bot")
                        .WithCustomId("chat_bot_prompt")
                        .AddTextInput("Enter Your Prompt Here:", "user_prompt", placeholder: "Hello World")
                        .AddTextInput("Set temperature 0-1 (higher = more random)", "temp", placeholder: "1.1");
            await cmd.RespondWithModalAsync(modal: modal.Build());
        }
        catch (Exception e)
        {
            
            Console.WriteLine(e);
        }
    }

    private async void HandleVideoCommand(SocketSlashCommand cmd)
    {
        var vidUrl = cmd.Data.Options.ElementAt(0).Value.ToString();
        var youtubeMatch = Regex.Match(vidUrl, @"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
        var vidId = youtubeMatch.Success ? youtubeMatch.Groups[1].Value : String.Empty;
        if (!String.IsNullOrEmpty(vidId))
        {
            Video vid;
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = Environment.GetEnvironmentVariable("YTAPI"),
                ApplicationName = this.GetType().ToString()
            });

            var videosListRequest = youtubeService.Videos.List("snippet,statistics");
            videosListRequest.Id = vidId;
            var videoListResponse = await videosListRequest.ExecuteAsync();

            if (videoListResponse.Items.Count > 0)
            {
                vid = videoListResponse.Items[0];
                Console.WriteLine("Title: " + vid.Snippet.Title);
                Console.WriteLine("View Count: " + vid.Statistics.ViewCount);
                Console.WriteLine("Description: " + vid.Snippet.Description);

                IForumChannel vidForumChan = (SocketForumChannel)await _client.GetChannelAsync(1106579191937384560);
                var post = await vidForumChan.CreatePostAsync(title: vid.Snippet.Title, ThreadArchiveDuration.OneDay, text: vidUrl);
                
                await post.SendMessageAsync($"Posted by: {cmd.User.Mention} \n Views: {vid.Statistics.ViewCount:n0} \n Uploaded By: {vid.Snippet.ChannelTitle}");
                await cmd.RespondAsync($"New Video: {post.Mention}");
            }
            else
            {
                Console.WriteLine("Invalid video ID");
                await cmd.RespondAsync("Invalid Video ID");
            }
        }
    }
    private async void ChatWarn(SocketSlashCommand command)
    {
        await command.RespondAsync("Hello, You will be awarded 5000 tokens and issued 1 tokens per 208 seconds back up to the 5000 token limit. This api costs money so play fair please.");

    }

    //token award system 
    //for now I say just add x tokens per hour up to max tokens.

    private async void PromptTokenCheck(SocketSlashCommand command)
    {
        string prompt = command.Data.Options.ElementAt(0).Value.ToString();

        var tokens = GPT3Tokenizer.Encode(prompt);
        command.RespondAsync($"Checking your prompt: {tokens.Count()} tokens");
    }
    private async void UserTokenCheck(SocketSlashCommand command)
    {
        using (var connection = new SqliteConnection("data Source=users.db"))
        {
            if (dbHelper.UserExistsInDB(connection, command.User.Id))
            {
                var user = dbHelper.GetUserFromUsersTable(connection, command.User.Id);
                await command.RespondAsync($"You have {user.AvailableTokens}");
            }
            else
            {
                await command.RespondAsync($"You haven't registered, try sending a message and you will automatically be registered");
            }
        }

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
            if (dbHelper.UserExistsInDB(_connection, command.User.Id))
            {
                var user = dbHelper.GetUserFromUsersTable(_connection, command.User.Id);
                string prompt = command.Data.Options.ElementAt(0).Value.ToString();

                Console.WriteLine($"User: {username} | prompt: {prompt}");

                var tokens = GPT3Tokenizer.Encode(prompt);
                user = dbHelper.GetUpdatedUser(_connection, user);
                if (user.AvailableTokens > tokens.Count())
                {

                    var modal = new ModalBuilder()
                        .WithTitle("Chat Bot")
                        .WithCustomId("chat_bot_prompt")
                        .AddTextInput("Enter Your Prompt Here:", "user_prompt", placeholder: "Hello World");
                    await command.RespondWithModalAsync(modal: modal.Build());


                    //Console.WriteLine("user has enough tokens");
                    //dbHelper.UpdateUserTokens(_connection, user, tokens.Count());

                    //var chat = _openAI.Chat.CreateConversation();
                    //chat.AppendUserInput(prompt);

                    //var channel = command.Channel;
                    //var response = new StringBuilder();

                    //response.Append($"{command.User.Username} asked: {prompt}\n\n");

                    ////iterate over the streaming response from the api and send it out in discord messages.
                    ////discord has a 2000 character limit so we need to split long messages into groups.
                    //int nextTarget = 0;
                    //int curMsgLength = 0;
                    //int curMsg = 0;


                    //Console.WriteLine();
                    //var messageStr = new StringBuilder();
                    //await foreach( var res in chat.StreamResponseEnumerableFromChatbotAsync())
                    //{
                    //    Console.Write(res);
                    //    messageStr.Append(res);
                    //}

                    
                    //string tempFilePath = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
                    //File.WriteAllText(tempFilePath, messageStr.ToString());
                    //await command.Channel.SendFileAsync(tempFilePath);
                    //File.Delete(tempFilePath);



                    




                    //var embed = new EmbedBuilder
                    //{
                    //    Title = "Hello World",
                    //    Description = "test"
                    //};

                    //var button = new ComponentBuilder().WithButton("button", "buttonId");

                    //embed.AddField("Hello World", "Hello world again").WithAuthor(command.User.Username);
                    //embed.AddField("title", "description").WithFooter("Footer").WithCurrentTimestamp();
                    //embed.AddField("See More", embed.Build());
                    



                    //await command.Channel.SendMessageAsync(embed: embed.Build(), components: button.Build());



                    //RestUserMessage newMsg = null;
                    //await foreach (var res in chat.StreamResponseEnumerableFromChatbotAsync())
                    //{
                    //    curMsgLength += res.Length;
                    //    response.Append(res.ToString());
                    //    Console.WriteLine(response.Length);
                    //    if (curMsgLength > 1900)
                    //    {
                    //        response = new StringBuilder();
                    //        response.Append(res.ToString());
                    //        curMsg++;
                    //        curMsgLength = 0;
                    //        await command.Channel.SendMessageAsync("...");
                    //        newMsg = await command.Channel.SendMessageAsync(response.ToString());
                    //    }
                    //    if (curMsg == 0)
                    //    {
                    //        if (response.Length > nextTarget)
                    //        {
                    //            nextTarget += 25;
                    //            await command.ModifyOriginalResponseAsync(msg => msg.Content = response.ToString());
                    //        }
                    //    }
                    //    else
                    //    {
                    //        await newMsg.ModifyAsync(msg => msg.Content = response.ToString());

                    //    }
                    //}
                    //await command.ModifyOriginalResponseAsync(msg => msg.Content = response.ToString());
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
            else
            {
                Console.WriteLine("User is not registered yet");
                int result = dbHelper.AddNewUserToUsersTable(_connection, command.User.Id, command.User.Username);
                //ChatWarn(command);
                await command.Channel.SendMessageAsync("Hello, You will be awarded 5000 tokens and issued 1 tokens per 208 seconds back up to the 5000 token limit. This api costs money so play fair please.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await command.Channel.SendMessageAsync("an error occurred, try again");
            await command.Channel.SendMessageAsync($"https://cdn.discordapp.com/attachments/936034644166598760/945888996356149288/image0.jpg");

        }
    }

    private async void GPTResponse()
    {

    }
    private async void RewardTokens(SqliteConnection conn)
    {
        //get all registered users
        //add x tokens over each user
        //208 seconds add one token
        //check if user has 5k tokens already
        //TODO check if user it at max tokens

        var users = dbHelper.GetAllUsers(conn);
        foreach (var user in users)
        {
            if (user.AvailableTokens >= 5000)
                continue;
            Console.WriteLine($"Adding token for user {user.Name}");
            dbHelper.AddUserTokens(conn, user, 1);
        }



    }
    private SocketGuildUser GetUserFromCommand(SocketSlashCommand command)
    {
        SocketSlashCommandDataOption user = command.Data.Options.ElementAt(0);
        SocketGuildUser guildUser = (SocketGuildUser)user.Value;
        return guildUser;
    }

    private async void ListRoles(SocketSlashCommand command)
    {
        var guildUser = GetUserFromCommand(command);
        var roles = guildUser.Roles;
        StringBuilder response = new();
        foreach (var role in roles)
        {
            //Don't put @everyone in this list
            response.Append($"\n{role.Name}");

        }
        await command.RespondAsync(response.ToString());
        var a = true;
    }
    private void AddRole(SocketSlashCommand command)
    {
        SocketSlashCommandDataOption user = command.Data.Options.ElementAt(0);
        SocketSlashCommandDataOption role = command.Data.Options.ElementAt(1);

        var a = true;
        //_guild.GetUser(user.Value);
        //command.Data.Options
        //_client.Rest.AddRoleAsync()
    }
    private void RemoveRoll(SocketSlashCommand command)
    {

    }
    private async void Remember(SocketSlashCommand command)
    {
        await command.RespondAsync($"https://cdn.discordapp.com/attachments/936034644166598760/945888996356149288/image0.jpg");
    }
    private async void Version(SocketSlashCommand command)
    {
        await command.RespondAsync($"Version: {_versionReporter.CurrentVersion.FullVersionNumberString}");
    }

    #endregion
}
