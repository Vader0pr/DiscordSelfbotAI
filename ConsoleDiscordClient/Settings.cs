using Discord.Gateway;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordSelfbotAI
{
    public class Settings
    {
        public string OpenAiApiKey { get; set; } = "OPEN AI API KEY";
        public string Token { get; set; } = "DISCORD TOKEN";
        public AiCharacter Character { get; set; } = new();
        public ulong?[] ServerBlacklist { get; set; } = { null };
        public int TokenLimit { get; set; } = 50;
        public int MessageLimit { get; set; } = 10;
        public PresenceProperties DiscordPresence { get; set; } = new PresenceProperties { Status = UserStatus.Online, Activity = new ActivityProperties { Name = "Minecraft", Type = ActivityType.Game } };
        public void Save()
        {
            File.WriteAllText("Settings.json", JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static Settings GetSettings()
        {
            if (File.Exists("Settings.json"))
            {
                var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
                if (settings is not null) return settings;
                else { new Settings().Save(); return new Settings(); }
            }
            else { new Settings().Save(); return new Settings(); }
        }
    }
}
