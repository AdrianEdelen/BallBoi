using BallBoi;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;


public class Program
{

    private DiscordSocketClient _client;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {



        //Things the bot can do: Ideas
        //music playlist, remove the embed, after x time, rewrite it to a regular

        var apiKey = Environment.GetEnvironmentVariable("APIKEY");
        Console.WriteLine(apiKey);
        Console.WriteLine($" Version: {VersionReporter.Version}");


        _client = new DiscordSocketClient();
        _client.Log += Log;
        _client.Ready += ClientReady;
        _client.SlashCommandExecuted += SlashCommandHandler;



        // Some alternative options would be to keep your token in an Environment Variable or a standalone file.
        // var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
        // var token = File.ReadAllText("token.txt");
        // var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

        var _interactionService = new InteractionService(_client.Rest);

        if (apiKey != "APIKEY")
        {
            await _client.LoginAsync(TokenType.Bot, apiKey);
            await _client.StartAsync();
        }
        else
        {
            Console.WriteLine("Apikey not found");
            Console.WriteLine("Please set the key in envuronment variables and restart the application");
        }




        async Task ClientReady()
        {
            //TODO: change all of this to just check if the command exists and build it if it doesn't.
            //this way new commands will automatically be deployed to the server on update.

            RegisterCommands();

        }

        //Block until the application is closed.
        await Task.Delay(-1);
    }
    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "remember":
                await command.RespondAsync($"https://cdn.discordapp.com/attachments/936034644166598760/945888996356149288/image0.jpg");
                break;
            case "version":
                await command.RespondAsync($"Version: {VersionReporter.Version}");
                break;
            case "add":
                break;
            case "remove":
                break;
            default:
                Console.WriteLine("Unknown Command recieved in SlashCommandhandler");
                break;
        }
        //await command.RespondAsync($"You executed {command.Data.Name}");

    }

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
                new SlashCommandBuilder().WithName("version").WithDescription("Display version/about information")
            };
            foreach (var slashCommand in slashCommandList)
            {
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
}

