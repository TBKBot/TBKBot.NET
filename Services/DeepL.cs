using System.Text.Json;

namespace TBKBot.Services
{
    public class DeepL
    {
        private static readonly string DEEPL_API_ENDPOINT = "https://api-free.deepl.com/v2/translate";

        public async Task<string> Translate(string text, string targetLang)
        {
            var config = new JSONReader();
            await config.ReadJSON();

            string apiKey = config.DeepLKey;

            using (HttpClient client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("auth_key", apiKey),
                    new KeyValuePair<string, string>("text", text),
                    new KeyValuePair<string, string>("target_lang", targetLang)
                });

                HttpResponseMessage response = await client.PostAsync(DEEPL_API_ENDPOINT, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseBody);

                var jsonResponse = JsonDocument.Parse(responseBody);

                string translatedText = jsonResponse.RootElement.GetProperty("translations")[0].GetProperty("text").GetString();

                return translatedText;
            }
        }
    }
}
