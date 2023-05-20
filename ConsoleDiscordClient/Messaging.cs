using Discord.Gateway;
using Discord;
using DiscordSelfbotAI.SelfbotMessages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSelfbotAI.SelfbotMessages
{
    public static class Messaging
    {
        public static void UseCommand(DiscordSocketClient client, MessageInfo messageInfo, List<DiscordMessage> contextMessages, Settings settings)
        {
            string[] messageArgs = messageInfo.Message.Split();
            switch (messageInfo.Message.Split()[0].ToLower())
            {
                case "!setcharacter":
                    List<string> argsList0 = messageArgs.ToList();
                    argsList0.RemoveAt(0);
                    string characterCommand = "";
                    foreach (string item in argsList0)
                    {
                        characterCommand = characterCommand + item + " ";
                    }
                    settings.Character.CharacterDescription = characterCommand;
                    settings.Save();
                    break;
                case "!getcharacter":
                    client.SendMessage(messageInfo.ChannelId, "Current character: " + settings.Character.CharacterDescription);
                    break;
                case "!setname":
                    List<string> argsList1 = messageArgs.ToList();
                    argsList1.RemoveAt(0);
                    string nameCommand = "";
                    foreach (string item in argsList1)
                    {
                        nameCommand = nameCommand + item + " ";
                    }
                    settings.Character.CharacterName = nameCommand;
                    settings.Save();
                    break;
                case "!getname":
                    client.SendMessage(messageInfo.ChannelId, "Current name: " + settings.Character.CharacterName);
                    break;
                case "!reset":
                    File.Delete("Messages.log");
                    contextMessages.Clear();
                    break;
                default:
                    break;
            }
        }
        public async static Task<Discord.DiscordMessage> SendMessageAsync(DiscordSocketClient client, MessageInfo messageInfo, List<SelfbotMessages.DiscordMessage> contextMessages, Settings settings)
        {
            if (!string.IsNullOrEmpty(messageInfo.Message) && messageInfo.UserType == DiscordUserType.User)
            {
                contextMessages.Add(new SelfbotMessages.DiscordMessage(messageInfo.ChannelId, messageInfo.Username, messageInfo.Message, false));
                if (messageInfo.Guild is not null && !settings.ServerBlacklist.Contains(messageInfo.Guild.Id)) return client.SendMessage(messageInfo.ChannelId, await Ai.SendAiRequestAsync(messageInfo, contextMessages, settings));
                else if (messageInfo.Guild is null) return client.SendMessage(messageInfo.ChannelId, await Ai.SendAiRequestAsync(messageInfo, contextMessages, settings));
                else throw new MessageSendingException("Error sending a message(Probably server is blacklisted in settings)");
            }
            else throw new MessageSendingException("Error sending a message(Probably replying to null message, only attachment message or message sent by a bot)");
        }
        public static async void ReceiveMessageAsync(DiscordSocketClient client, MessageInfo messageInfo, List<SelfbotMessages.DiscordMessage> contextMessages, Settings settings)
        {
            try
            {
                if (messageInfo.Guild is not null && messageInfo.UserType == DiscordUserType.User) Console.WriteLine($"<{client.GetGuild(messageInfo.Guild.Id).Name}>/{client.GetChannel(messageInfo.ChannelId)} {messageInfo.Username}: {messageInfo.Message}");
                else Console.WriteLine($"{messageInfo.Username}: {messageInfo.Message}");

                if (!messageInfo.Message.StartsWith("!"))
                {
                    if (messageInfo.Username == client.User.Username && !string.IsNullOrEmpty(messageInfo.Message) && messageInfo.UserType == DiscordUserType.User) contextMessages.Add(new SelfbotMessages.DiscordMessage(messageInfo.ChannelId, messageInfo.Username, messageInfo.Message, true));
                    else { try { await SendMessageAsync(client, messageInfo, contextMessages, settings); } catch (Exception ex) { Console.WriteLine(ex.Message); } }
                }
                else
                {
                    UseCommand(client, messageInfo, contextMessages, settings);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            foreach (var item in contextMessages.DistinctBy(x => x.ChannelId))
            {
                var currentChannelMessages = contextMessages.Where(x => x.ChannelId == item.ChannelId).ToList().OrderBy(x => x.Id).ToList();
                while (settings.MessageLimit < currentChannelMessages.Count)
                {
                    currentChannelMessages.RemoveAt(0);
                }
                contextMessages = currentChannelMessages;
            }
            File.WriteAllText("Messages.log", JsonConvert.SerializeObject(contextMessages, Formatting.Indented));
        }
    }

    [Serializable]
    public class MessageSendingException : Exception
    {
        public MessageSendingException() { }
        public MessageSendingException(string message) : base(message) { }
        public MessageSendingException(string message, Exception inner) : base(message, inner) { }
        protected MessageSendingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
