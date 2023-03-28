using Discord;
using Discord.Gateway;
using Discord.Media;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using System;
using System.Threading;
using System.Xml;
using OpenAI.GPT3.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.GPT3;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Newtonsoft.Json;

namespace DiscordSelfbotAI
{
    class Program
    {
        static List<DiscordMessageListed> allMessages;
        static Settings settings;
        static void Main()
        {
            if (File.Exists("Messages.log")) allMessages = JsonConvert.DeserializeObject<List<DiscordMessageListed>>(File.ReadAllText("Messages.log"));
            else allMessages = new List<DiscordMessageListed>();

            if (File.Exists("Settings.json")) settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
            else { settings = new Settings(); settings.Save(); }

            DiscordSocketClient client = new DiscordSocketClient();
            client.OnLoggedIn += Client_OnLoggedIn;
            client.CreateCommandHandler(",");
            try
            {
                client.Login(settings.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Thread.Sleep(-1);
        }

        private static void Client_OnLoggedIn(DiscordSocketClient client, LoginEventArgs args)
        {
            Console.WriteLine("Logged in");
            client.OnMessageReceived += Client_OnMessageReceived;
            client.UpdatePresence(settings.DiscordPresence);
        }

        private static async void Client_OnMessageReceived(DiscordSocketClient client, MessageEventArgs args)
        {
            try
            {
                if (args.Message.Guild is not null && args.Message.Author.User.Type == DiscordUserType.User) Console.WriteLine($"<{client.GetGuild(args.Message.Guild.Id).Name}>/{client.GetChannel(args.Message.Channel.Id)} {args.Message.Author.User}: {args.Message.Content}");
                else Console.WriteLine($"{args.Message.Author.User}: {args.Message.Content}");

                if (args.Message.Author.User.Username == client.User.Username && !string.IsNullOrEmpty(args.Message.Content) && !args.Message.Content.StartsWith("!") && args.Message.Author.User.Type == DiscordUserType.User)
                {
                    allMessages.Add(new DiscordMessageListed(args.Message.Channel.Id, args.Message.Author.User.Username, args.Message.Content, true));
                }
                if (args.Message.Author.User.Username != client.User.Username && !string.IsNullOrEmpty(args.Message.Content) && !args.Message.Content.StartsWith("!") && args.Message.Author.User.Type == DiscordUserType.User)
                {
                    allMessages.Add(new DiscordMessageListed(args.Message.Channel.Id, args.Message.Author.User.Username, args.Message.Content, false));
                    if (args.Message.Guild is not null && !settings.ServerBlacklist.Contains(args.Message.Guild.Id)) client.SendMessage(args.Message.Channel.Id, await GetChatGPTReplyAsync(args.Message.Content, args.Message.Author.User.Username, args.Message.Channel.Id, settings.CharacterDescription, settings.CharacterName));
                    else if (args.Message.Guild is null) client.SendMessage(args.Message.Channel.Id, await GetChatGPTReplyAsync(args.Message.Content, args.Message.Author.User.Username, args.Message.Channel.Id, settings.CharacterDescription, settings.CharacterName));
                }
                else
                {
                    switch (args.Message.Content.Split()[0].ToLower())
                    {
                        case "!setcharacter":
                            List<string> argsList0 = args.Message.Content.Split().ToList();
                            argsList0.RemoveAt(0);
                            string characterCommand = "";
                            foreach (string item in argsList0)
                            {
                                characterCommand = characterCommand + item + " ";
                            }
                            settings.CharacterDescription = characterCommand;
                            break;
                        case "!getcharacter":
                            client.SendMessage(args.Message.Channel.Id, "Current character: " + settings.CharacterDescription);
                            break;
                        case "!setname":
                            List<string> argsList1 = args.Message.Content.Split().ToList();
                            argsList1.RemoveAt(0);
                            string nameCommand = "";
                            foreach (string item in argsList1)
                            {
                                nameCommand = nameCommand + item + " ";
                            }
                            settings.CharacterName = nameCommand;
                            settings.Save();
                            break;
                        case "!getname":
                            client.SendMessage(args.Message.Channel.Id, "Current name: " + settings.CharacterName);
                            break;
                        case "!reset":
                            File.Delete("Messages.log");
                            allMessages.Clear();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            File.WriteAllText("Messages.log", JsonConvert.SerializeObject(allMessages, Newtonsoft.Json.Formatting.Indented));
        }
        private static async Task<string> GetChatGPTReplyAsync(string message, string username, ulong channelId, string characterDescription = "", string characterName = "You")
        {

            List<ChatMessage> messages = new List<ChatMessage>();
            List<DiscordMessageListed> discordMessages = allMessages.Where(x => x.ChannelId == channelId).OrderBy(x => x.Id).ToList();
            File.WriteAllText("Prompt.log", JsonConvert.SerializeObject(allMessages, Newtonsoft.Json.Formatting.Indented));
            messages.Add(ChatMessage.FromUser(characterDescription));
            string prompt = characterDescription + "\n";
            for (int i = 0; i < discordMessages.Count; i++)
            {
                if (discordMessages[i].MessagedBySelf == false) prompt = prompt + $"{discordMessages[i].Username}:{discordMessages[i].Content}\n";
                if (discordMessages[i].MessagedBySelf == true) prompt = prompt + $"{characterName}:{discordMessages[i].Content}\n";
            }
            prompt = prompt + $"{characterName}:";
            File.WriteAllText("Prompt.log", prompt);
            var openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = settings.OpenAiApiKey
            });
            var completionResult = await openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
            {
                Prompt = prompt,
                Model = Models.TextDavinciV3,
                MaxTokens = 30
            });

            if (completionResult.Successful)
            {
                Console.WriteLine(completionResult.Choices.FirstOrDefault().Text);
                return completionResult.Choices.FirstOrDefault().Text;
            }
            else
            {
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
                throw new Exception();
            }
        }
    }
    public class DiscordMessageListed
    {
        public int Id
        {
            get;
            private set;
        }
        public void SetId()
        {
            LastId++;
            Id = LastId;
        }
        public bool MessagedBySelf { get; set; }
        public ulong ChannelId { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        private static int LastId { get; set; } = 0;
        public DiscordMessageListed(ulong channelId, string username, string content, bool messagedBySelf)
        {
            SetId();
            ChannelId = channelId;
            Username = username;
            Content = content;
            MessagedBySelf = messagedBySelf;
        }
    }
    public class Settings
    {
        public string OpenAiApiKey { get; set; } = "OPEN AI API KEY";
        public string Token { get; set; } = "DISCORD TOKEN";
        public string CharacterName { get; set; } = "You";
        public string CharacterDescription { get; set; } = "";
        public ulong?[] ServerBlacklist { get; set; } = { null };
        public Discord.Gateway.PresenceProperties DiscordPresence { get; set; } = new PresenceProperties { Status = UserStatus.Online, Activity = new ActivityProperties { Name = "Minecraft", Type = ActivityType.Game } };
        public void Save()
        {
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }
    }
}