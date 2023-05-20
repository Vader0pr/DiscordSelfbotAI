using Newtonsoft.Json;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordSelfbotAI.SelfbotMessages;
using OpenAI.GPT3.ObjectModels.ResponseModels;
using Discord.Gateway;
using Discord;

namespace DiscordSelfbotAI
{
    public static class Ai
    {
        public static async Task<string> SendAiRequestAsync(MessageInfo messageInfo, List<SelfbotMessages.DiscordMessage> contextMessages, Settings settings)
        {
            var character = settings.Character;
            List<SelfbotMessages.DiscordMessage> discordMessages = contextMessages.Where(x => x.ChannelId == messageInfo.ChannelId).OrderBy(x => x.Id).ToList();
            File.WriteAllText("Prompt.log", JsonConvert.SerializeObject(contextMessages, Formatting.Indented));
            string prompt = character.CharacterDescription + "\n";
            for (int i = 0; i < discordMessages.Count; i++)
            {
                if (discordMessages[i].MessagedBySelf == false) prompt = prompt + $"{discordMessages[i].Username}:{discordMessages[i].Content}\n";
                if (discordMessages[i].MessagedBySelf == true) prompt = prompt + $"{character.CharacterName}:{discordMessages[i].Content}\n";
            }
            prompt = prompt + $"{character.CharacterName}:";
            File.WriteAllText("Prompt.log", prompt);
            var response = await GetAiRosponseAsync(settings, prompt);
            CheckAiResponse(response);
            return response.Choices.FirstOrDefault().Text;
        }
        public static async Task<CompletionCreateResponse?> GetAiRosponseAsync(Settings settings, string prompt)
        {
            var openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = settings.OpenAiApiKey
            });
            var completionResult = await openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
            {
                Prompt = prompt,
                Model = Models.TextDavinciV3,
                MaxTokens = settings.TokenLimit
            });
            return completionResult;
        }
        public static void CheckAiResponse(CompletionCreateResponse? completionResult)
        {
            if (completionResult.Successful)
            {
                Console.WriteLine("(Response generated succesfully)");
            }
            else
            {
                if (completionResult.Error is null)
                {
                    Console.WriteLine("Unknown Error");
                }
                Console.WriteLine($"{completionResult.Error.Code}: {completionResult.Error.Message}");
            }
        }
    }
    public class AiCharacter
    {
        public string CharacterName { get; set; } = "You";
        public string CharacterDescription { get; set; } = "";
    }

}
