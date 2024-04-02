using Newtonsoft.Json;

namespace TBKBot
{
    internal class JSONReader
    {
        public string Token { get; set; }
        public string DeepLKey { get; set; }
        public string Prefix { get; set; }
        public ulong? WelcomeChannel { get; set; }
        public string MongoUrl { get; set; }

        public async Task ReadJSON()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                this.Token = data.Token;
                this.DeepLKey = data.DeepLKey;
                this.Prefix = data.Prefix;
                this.WelcomeChannel = data.WelcomeChannel;
                this.MongoUrl = data.MongoUrl;
            }
        }
    }

    internal sealed class JSONStructure
    {
        public string Token { get; set; }
        public string DeepLKey { get; set; }
        public string Prefix { get; set; }
        public ulong? WelcomeChannel { get; set; }
        public string MongoUrl { get; set; }
    }
}
