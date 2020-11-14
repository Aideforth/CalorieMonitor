using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Models;
using CalorieMonitor.Data.Interfaces;
using CalorieMonitor.End2End.Implementations;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.End2End
{
    public class BaseControllerEnd2End : IClassFixture<WebApplicationFactory<Startup>>
    {
        protected readonly WebApplicationFactory<Startup> _factory;

        public BaseControllerEnd2End(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.json");

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, conf) =>
                {
                    conf.AddJsonFile(configPath);
                });
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton<IDbConnectionProvider>(sp => new SqlLiteConnectionProvider());
                    services.AddTransient<IUserDAO, UserDAOWrapper>();
                    services.AddTransient<IMealEntryDAO, MealEntryDAOWrapper>();
                    services.AddHangfire(config =>
                    {
                        config.UseMemoryStorage();
                    });
                });

            });
        }

        protected static async Task ValidateUserResult(HttpResponseMessage response, User user)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            ApiObjectResponse<User> requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjectResponse<User>>(responseString);
            ValidateApiObjectResponse(requestResponse);
            Assert.Equal(user.UserName, requestResponse.Data.UserName);
            Assert.Equal(user.FirstName, requestResponse.Data.FirstName);
            Assert.Equal(user.LastName, requestResponse.Data.LastName);
            Assert.Equal(user.EmailAddress, requestResponse.Data.EmailAddress);
            Assert.Equal(user.Role, requestResponse.Data.Role);
            Assert.Equal(user.DailyCalorieLimit, requestResponse.Data.DailyCalorieLimit);
            Assert.Equal(user.Id, requestResponse.Data.Id);

            response.Dispose();
        }

        protected static async Task ValidateMealEntryResult(HttpResponseMessage response, MealEntry entry)
        {
            string responseString = await response.Content.ReadAsStringAsync();
            ApiObjectResponse<MealEntry> requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjectResponse<MealEntry>>(responseString);
            ValidateApiObjectResponse(requestResponse);
            Assert.NotNull(requestResponse.Data.EntryUser);
            Assert.NotNull(requestResponse.Data.EntryCreator);
            Assert.Equal(entry.EntryUser.EmailAddress, requestResponse.Data.EntryUser.EmailAddress);
            Assert.Equal(entry.EntryUser.Role, requestResponse.Data.EntryUser.Role);
            Assert.Equal(entry.EntryCreator.EmailAddress, requestResponse.Data.EntryCreator.EmailAddress);
            Assert.Equal(entry.EntryCreator.Role, requestResponse.Data.EntryCreator.Role);
            Assert.Equal(entry.Calories, requestResponse.Data.Calories);
            Assert.Equal(entry.EntryDateTime.ToUniversalTime(), requestResponse.Data.EntryDateTime.ToUniversalTime());
            Assert.Equal(entry.Text, requestResponse.Data.Text);
            Assert.Equal(entry.WithInDailyLimit, requestResponse.Data.WithInDailyLimit);
            Assert.Equal(entry.CaloriesStatus, requestResponse.Data.CaloriesStatus);
            Assert.Equal(entry.Id, requestResponse.Data.Id);

            response.Dispose();
        }
        protected static void ValidateApiObjectResponse<T>(ApiObjectResponse<T> requestResponse)
        {
            Assert.NotNull(requestResponse);
            Assert.True(requestResponse.IsSuccessful);
            Assert.Equal("Successful", requestResponse.Message);
            Assert.NotNull(requestResponse.Data);
        }

        protected static void ValidateLogin(ApiObjectResponse<TokenInfo> requestResponse)
        {
            ValidateApiObjectResponse(requestResponse);
            Assert.NotNull(requestResponse.Data.Token);
        }

    }
}
