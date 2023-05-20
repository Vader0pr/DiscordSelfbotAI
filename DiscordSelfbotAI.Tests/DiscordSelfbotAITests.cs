using Discord.Gateway;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Security.Cryptography;

namespace DiscordSelfbotAI.Tests
{
    public class DiscordSelfbotAITests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task GetAiReplyAsync_Test()
        {
            try
            {
                var response = await Ai.GetAiRosponseAsync(Settings.GetSettings(), string.Empty);
                Assert.IsTrue(response.Successful);
                Assert.IsInstanceOf<string>(response.Choices.FirstOrDefault().Text);
            }
            catch (Exception ex)
            {
                Assert.Warn(ex.Message);
            }
        }
        [Test]
        public void ReceiveMessage_Test()
        {
            var settings = Settings.GetSettings();
            DiscordSocketClient client = new DiscordSocketClient();
            client.OnLoggedIn += ReceiveMessage_Test_Client_OnLoggedIn;
            try
            {
                client.Login(settings.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void ReceiveMessage_Test_Client_OnLoggedIn(DiscordSocketClient client, LoginEventArgs args)
        {
            Assert.Pass();
            client.Dispose();
        }
        [Test]
        public void SendMessage_Test()
        {
            var settings = Settings.GetSettings();
            DiscordSocketClient client = new DiscordSocketClient();
            client.OnLoggedIn += SendMessage_Test_Client_OnLoggedIn; ;
            try
            {
                client.Login(settings.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void SendMessage_Test_Client_OnLoggedIn(DiscordSocketClient client, LoginEventArgs args)
        {
            var messageSent = Messaging.SendMessageAsync(client, new MessageInfo(), new List<DiscordMessage>(), Settings.GetSettings()).Result;
            Assert.IsInstanceOf<Discord.DiscordMessage>(messageSent);
            client.Dispose();
        }
        [Test]
        public void GetSettings_Test()
        {
            Assert.IsInstanceOf(typeof(Settings), Settings.GetSettings());
        }
    }
}