using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using WebTests.DTOs;
using DotNetEnv;

namespace WebTests.Services
{
    public class YandexGptService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public YandexGptService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _configuration = config;
        }

        public async Task<string> Test()
        {
            Env.Load();

            var apiKey = Environment.GetEnvironmentVariable("ApiKey");
            var folderId = Environment.GetEnvironmentVariable("FolderId");

            var requestBody = new
            {
                modelUri = $"gpt://{folderId}/yandexgpt/latest",
                completionOptions = new
                {
                    stream = false,
                    temperature = 0.3,
                    maxTokens = "100"
                },
                messages = new[]
            {
                new
                {
                    role = "user",
                    text = "Привет! Ответь одной короткой фразой."
                }
            }
            };

            var json = System.Text.Json.JsonSerializer.Serialize(requestBody);

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://llm.api.cloud.yandex.net/foundationModels/v1/completion"
            );

            request.Headers.Add("Authorization", $"Api-Key {apiKey}");

            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            // Для отладки: покажет ошибку, если API не отвечает
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(responseText);

            return responseText;
        }

        public async Task<TestDto> GenerateTest(string userPrompt, int count)
        {
            Env.Load();

            var apiKey = Environment.GetEnvironmentVariable("ApiKey");
            var folderId = Environment.GetEnvironmentVariable("FolderId");

            var prompt = @$"
            Создай тест в JSON формате.

            Верни ТОЛЬКО JSON.
            Без markdown.
            Без ```json.
            Без пояснений.
            Если isMultiple = false, значит в вопросе только один правильный вариант ответа,
            ставь isMultiple = true, чтобы указать несколько правильных вариантов ответа.

            Структура JSON:

            {{
              ""title"": ""string"",
              ""description"": ""string"",
              ""questions"": [
                {{
                  ""text"": ""string"",
                  ""options"": [
                    {{
                      ""text"": ""string"",
                      ""isCorrect"": boolean
                    }}
                  ],
                  ""isMultiple"": ""boolean""
                }}
              ]
            }}

            Тема теста:
            {userPrompt}

            Количество вопросов:
            {count}
            ";

            var body = new
            {
                modelUri = $"gpt://{folderId}/yandexgpt/latest",
                completionOptions = new
                {
                    stream = false,
                    temperature = 0.3,
                    maxTokens = 4000
                },
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        text = prompt
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://llm.api.cloud.yandex.net/foundationModels/v1/completion");

            request.Headers.Add("Authorization", $"Api-Key {apiKey}");

            request.Content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            dynamic result = JsonConvert.DeserializeObject(json);

            var generatedTest = result.result.alternatives[0].message.text.ToString();

            generatedTest = generatedTest
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var test = JsonConvert.DeserializeObject<TestDto>(generatedTest);

            return test;

            //return result.result.alternatives[0].message.text.ToString();
        }
    }
}
