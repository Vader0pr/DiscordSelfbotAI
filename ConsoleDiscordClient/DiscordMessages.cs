using Discord;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 

namespace DiscordSelfbotAI.SelfbotMessages
{
    public class DiscordMessage
    {
        public int Id { get; private set; }
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
        public DiscordMessage(ulong channelId, string username, string content, bool messagedBySelf)
        {
            SetId();
            ChannelId = channelId;
            Username = username;
            Content = content;
            MessagedBySelf = messagedBySelf;
        }
    }
    public class MessageInfo
    {
        public string Message { get; set; }
        public string Username { get; set; }
        public ulong ChannelId { get; set; }
        public MinimalGuild Guild { get; set; }
        public DiscordUserType UserType { get; set; }
        public MessageInfo()
        {
            new MessageInfo();
        }
        public MessageInfo(string message, string username, ulong channelId, MinimalGuild guild, DiscordUserType userType)
        {
            Message = message;
            Username = username;
            ChannelId = channelId;
            Guild = guild;
            UserType = userType;
        }
    }
    public static class DiscordMessages
    {
        public static List<DiscordMessage> GetDiscordMessages()
        {
            if (File.Exists("Messages.log"))
            {
                var messages = JsonConvert.DeserializeObject<List<DiscordMessage>>(File.ReadAllText("Messages.log"));
                if (messages is not null) return messages;
                else return new List<DiscordMessage>();
            }
            else return new List<DiscordMessage>();
        }
    }
}
