using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Logic.Interfaces;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CalorieMonitor.Logic.Implementations
{
    public class CalorieProviderService : ICalorieProviderService
    {
        readonly HttpClient client;
        readonly ILogManager logManager;
        readonly ICalorieProviderConfig calorieProviderConfig;
        public CalorieProviderService(HttpClient client, ILogManager logManager, ICalorieProviderConfig calorieProviderConfig)
        {
            this.client = client ?? throw new ArgumentNullException("client");
            this.logManager = logManager ?? throw new ArgumentNullException("logManager");
            this.calorieProviderConfig = calorieProviderConfig ?? throw new ArgumentNullException("calorieProviderConfig");
        }
        public async Task<CalorieServiceResult> GetCalorieResultAsync(string text)
        {
            CalorieServiceResult serviceResult = null;
            string url = calorieProviderConfig.NutrientsUrl;

            string requestJson = $"{{ \"query\" : \"{text}\"}}";
            var content = new StringContent(requestJson, System.Text.Encoding.UTF8, "application/json");

            client.DefaultRequestHeaders.Add("x-app-id", calorieProviderConfig.ApiId);
            client.DefaultRequestHeaders.Add("x-app-key", calorieProviderConfig.ApiKey);

            using (var response = await client.PostAsync(url, content))
            {
                if(response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new CalorieServiceResult();
                }
                if (!response.IsSuccessStatusCode)
                {
                    logManager.LogMessage($"{url} :: Request Failed with {response.ReasonPhrase}");
                    throw new FailureException($"Request Failed with {response.ReasonPhrase}");
                }

                string responseJson = await response.Content.ReadAsStringAsync();
                serviceResult = JsonConvert.DeserializeObject<CalorieServiceResult>(responseJson);

                logManager.LogMessage($"{url} :: Request Successful with {responseJson}");
            }
            return serviceResult;
        }
    }
}
