using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Enums;
using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.End2End.Authenticated
{
    public class MealsControllerEnd2End : BaseAuthenticatedControllerEnd2End
    {
        public MealsControllerEnd2End(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Theory]
        [InlineData(1, 2, UserRole.Admin)]
        [InlineData(3, 3, UserRole.Regular)]
        [InlineData(5, 4, UserRole.UserManager)]
        public async Task Get_GetMealEntry_EndpointReturnSuccessAndValidateContent(int id, int userId, UserRole role)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.GetAsync($"/api/meals/{id}");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateMealEntryResult(response, new MealEntry
            {
                Calories = 100,
                CaloriesStatus = CaloriesStatus.CustomerProvided,
                EntryCreator = new User { EmailAddress = $"email{userId}@gmail.com", Role = role },
                EntryDateTime = new DateTime(2020, 10, 20),
                EntryUser = new User { EmailAddress = $"email{userId}@gmail.com", Role = role },
                Id = id,
                Text = $"Text{id}",
                WithInDailyLimit = true
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        public async Task Delete_DeleteOtherMealEntry_EndpointReturnSuccessAndValidateContent(int id)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.DeleteAsync($"/api/meals/{id}");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            string responseString = await response.Content.ReadAsStringAsync();
            ApiResponse requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse>(responseString);
            Assert.NotNull(requestResponse);
            Assert.True(requestResponse.IsSuccessful);
            Assert.Equal("Successful", requestResponse.Message);

            response.Dispose();
        }

        [Theory]
        [InlineData(null, 99, UserRole.Admin)]
        [InlineData("1", 0, UserRole.Admin)]
        [InlineData("3", 0, UserRole.Regular)]
        [InlineData("4", 99, UserRole.UserManager)]
        public async Task Post_CreateOfficerAccount_EndpointReturnSuccessAndValidateContent(string userId, double calories, UserRole role)
        {
            // Arrange
            var client = _factory.CreateClient();
            string json = $"{{\"Text\": \"Text{userId ?? "2"}\",\"Calories\": {calories},\"EntryDateTime\": \"2020-10-20T23:11:11.0000000\"";
            if (!string.IsNullOrEmpty(userId))
            {
                json += $", \"EntryUser\": {{\"Id\":\"{userId}\"}}";
            }
            json += "}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.PostAsync("/api/meals", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateMealEntryResult(response, new MealEntry
            {
                Calories = calories,
                CaloriesStatus = calories > 0 ? CaloriesStatus.CustomerProvided : CaloriesStatus.Pending,
                EntryCreator = new User { EmailAddress = $"email2@gmail.com", Role = UserRole.Admin },
                EntryDateTime = new DateTime(2020, 10, 20, 23, 11, 11),
                EntryUser = new User { EmailAddress = $"email{userId ?? "2"}@gmail.com", Role = role },
                Id = 11,
                Text = $"Text{userId ?? "2"}",
                WithInDailyLimit = calories > 0
            });
        }

        [Theory]
        [InlineData(1, 2, 100, UserRole.Admin)]
        [InlineData(3, 3, 0, UserRole.Regular)]
        [InlineData(5, 4, 100, UserRole.UserManager)]
        public async Task PatchMealEntry_EndpointReturnSuccessAndValidateContent(long id, long userId, double calories, UserRole role)
        {
            // Arrange
            var client = _factory.CreateClient();
            string json = $"{{\"Text\": \"Text{id}\",\"Calories\": {calories}}}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.PatchAsync($"/api/meals/{id}", content);

            // Assert
            await ValidateMealEntryResult(response, new MealEntry
            {
                Calories = calories,
                CaloriesStatus = calories > 0 ? CaloriesStatus.CustomerProvided : CaloriesStatus.Pending,
                EntryCreator = new User { EmailAddress = $"email{userId}@gmail.com", Role = role },
                EntryDateTime = new DateTime(2020, 10, 20),
                EntryUser = new User { EmailAddress = $"email{userId}@gmail.com", Role = role },
                Id = id,
                Text = $"Text{id}",
                WithInDailyLimit = true
            });
        }


        [Fact]
        public async Task SearchMealEntry_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.GetAsync($"/api/meals?filter=(Calories gt '30') AND (EntryUser.Role eq 'Admin')&startIndex=1&limit=10");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            string responseString = await response.Content.ReadAsStringAsync();
            ApiObjectResponse<SearchResult<MealEntry>> requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjectResponse<SearchResult<MealEntry>>>(responseString);
            ValidateApiObjectResponse(requestResponse);
            Assert.NotNull(requestResponse.Data.Results);
            Assert.Equal(6, requestResponse.Data.TotalCount);
            Assert.True(requestResponse.Data.Results.All(b => b.EntryUser.Role == UserRole.Admin));

            response.Dispose();
        }

        [Fact]
        public async Task SearchOwnMealEntry_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client, "user3");
            var response = await client.GetAsync($"/api/meals?filter=(Calories gt '30') AND (EntryUser.DailyCalorieLimit gt '30')&startIndex=1&limit=10");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            string responseString = await response.Content.ReadAsStringAsync();
            ApiObjectResponse<SearchResult<MealEntry>> requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjectResponse<SearchResult<MealEntry>>>(responseString);
            ValidateApiObjectResponse(requestResponse);
            Assert.Equal(2, requestResponse.Data.TotalCount);
            Assert.NotNull(requestResponse.Data.Results);
            Assert.True(requestResponse.Data.Results.All(b => b.EntryUser.Role == UserRole.Regular && b.EntryUser.Id == 3));

            response.Dispose();
        }
    }
}
