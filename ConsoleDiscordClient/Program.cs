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
using DiscordSelfbotAI.SelfbotMessages;
using System.Threading.Channels;

namespace DiscordSelfbotAI
{
    class Program
    {
        static int currentClientSession = 0;
        static List<SelfbotMessages.DiscordMessage> contextMessages = DiscordMessages.GetDiscordMessages();
        static Settings settings = Settings.GetSettings();
        static List<DiscordSocketClient> runningClients = new();
        static void Main()
        {
            DiscordSocketClient client = new DiscordSocketClient();
            client.OnLoggedIn += Client_OnLoggedIn;
            client.OnMessageReceived += Client_OnMessageReceived;
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
            client.UpdatePresence(settings.DiscordPresence);
        }

        private static async void Client_OnMessageReceived(DiscordSocketClient client, MessageEventArgs args)
        {
            var messageInfo = new MessageInfo(args.Message.Content, args.Message.Author.User.Username, args.Message.Channel.Id, args.Message.Guild, args.Message.Author.User.Type);
            Messaging.ReceiveMessageAsync(client, messageInfo, contextMessages, settings);
        }
    }
}