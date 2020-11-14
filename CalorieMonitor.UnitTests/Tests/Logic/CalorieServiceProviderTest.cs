using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Interfaces;
using CalorieMonitor.Logic.Implementations;
using CalorieMonitor.Logic.Interfaces;
using CalorieMonitor.UnitTests.Mocks.External;
using CalorieMonitor.UnitTests.Mocks.Logic;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Logic
{
    public class CalorieProviderServiceTest
    {
        readonly MockHttpMessageHandler mockHttpMessageHandler;
        readonly HttpClient httpClient;
        readonly MockLogManager mockLogManager;
        readonly ICalorieProviderService calorieServiceProvider;
        readonly ICalorieProviderConfig calorieProviderConfig;

        public CalorieProviderServiceTest()
        {
            mockHttpMessageHandler = new MockHttpMessageHandler();
            mockLogManager = new MockLogManager();
            calorieProviderConfig = new CalorieProviderConfig
            {
                ApiId = "apiId",
                NutrientsUrl = "http://CalorieUrl.com",
                ApiKey = "authKey"
            };
            httpClient = new HttpClient(mockHttpMessageHandler.Object);

            calorieServiceProvider = new CalorieProviderService(httpClient, mockLogManager.Object, calorieProviderConfig);
        }

        [Fact]
        public void Constructor_NullHttpClientArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new CalorieProviderService(null, mockLogManager.Object, calorieProviderConfig));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("client", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullILogManagerArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new CalorieProviderService(httpClient, null, calorieProviderConfig));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("logManager", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public void Constructor_NullICalorieProviderConfigArgument_ThrowsException()
        {
            Exception exception = Record.Exception(() => new CalorieProviderService(httpClient, mockLogManager.Object, null));
            Assert.IsType<ArgumentNullException>(exception);
            Assert.Equal("calorieProviderConfig", (exception as ArgumentNullException).ParamName);
        }

        [Fact]
        public async Task GetCalorieResultAsync_UnSuccessfulHttpRequest_ThrowFailureException()
        {
            string reason = "Failed to get calorie result";

            HttpResponseMessage message = new HttpResponseMessage
            {
                ReasonPhrase = reason,
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            };
            mockHttpMessageHandler.MockSendAsync(calorieProviderConfig.NutrientsUrl, message);
            mockLogManager.MockLogMessage($"{calorieProviderConfig.NutrientsUrl} :: Request Failed with {reason}");


            Exception exception = await Record.ExceptionAsync(() => calorieServiceProvider.GetCalorieResultAsync("text"));

            Assert.IsType<FailureException>(exception);
            RunVerification();
        }

        [Fact]
        public async Task GetCalorieResultAsync_SuccessfulHttpRequest_ReturnsCalorieServiceResult()
        {
            string responseString = "{\"foods\":[{\"food_name\": \"fried rice\",\"serving_weight_grams\": 137,\"nf_calories\": 238.38},{\"food_name\": \"turkey\",\"serving_weight_grams\": 113.4,\"nf_calories\": 214.33}]}";

            HttpResponseMessage message = new HttpResponseMessage
            {
                Content = new StringContent(responseString),
                StatusCode = System.Net.HttpStatusCode.OK
            };
            mockHttpMessageHandler.MockSendAsync(calorieProviderConfig.NutrientsUrl, message);
            mockLogManager.MockLogMessage($"{calorieProviderConfig.NutrientsUrl} :: Request Successful with {responseString}");

            CalorieServiceResult response = await calorieServiceProvider.GetCalorieResultAsync("text");

            Assert.NotNull(response);
            Assert.NotNull(response.Foods);
            Assert.Equal(2, response.Foods.Count);
            Assert.Equal("fried rice", response.Foods[0].FoodName);
            Assert.Equal(137, response.Foods[0].ServingWeightGrams);
            Assert.Equal(238.38, response.Foods[0].Calories);
            Assert.Equal("turkey", response.Foods[1].FoodName);
            Assert.Equal(113.4, response.Foods[1].ServingWeightGrams);
            Assert.Equal(214.33, response.Foods[1].Calories);

            RunVerification();
        }

        private void RunVerification()
        {
            mockHttpMessageHandler.RunVerification();
            mockLogManager.RunVerification();
        }
    }
}