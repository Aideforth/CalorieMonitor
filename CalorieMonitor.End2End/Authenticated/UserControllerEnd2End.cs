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
    public class UserControllerEnd2End : BaseAuthenticatedControllerEnd2End
    {
        public UserControllerEnd2End(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task GetOwnUser_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.GetAsync("/api/users/own");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateUserResult(response, new User
            {
                UserName = "Aide4th",
                FirstName = "Aide",
                LastName = "New",
                EmailAddress = $"email2@gmail.com",
                DailyCalorieLimit = 100,
                Role = UserRole.Admin,
                Id = 2
            });
        }

        [Theory]
        [InlineData(1, UserRole.Admin)]
        [InlineData(3, UserRole.Regular)]
        [InlineData(4, UserRole.UserManager)]
        public async Task Get_GetOtherUser_EndpointReturnSuccessAndValidateContent(int id, UserRole role)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.GetAsync($"/api/users/{id}");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateUserResult(response, new User
            {
                UserName = $"user{id}",
                FirstName = "Name",
                LastName = "New",
                EmailAddress = $"email{id}@gmail.com",
                DailyCalorieLimit = 100,
                Role = role,
                Id = id
            });
        }

        [Fact]
        public async Task Delete_DeleteOtherUser_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.DeleteAsync($"/api/users/5");

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
        [InlineData(UserRole.Admin)]
        [InlineData(UserRole.UserManager)]
        public async Task Post_CreateOfficerAccount_EndpointReturnSuccessAndValidateContent(UserRole role)
        {
            // Arrange
            var client = _factory.CreateClient();
            string unique = Guid.NewGuid().ToString();
            string json = $"{{\"firstName\": \"Name\",\"lastName\": \"New\",\"username\": \"{unique}\", \"emailaddress\": \"{unique}@gmail.com\"," +
                $"\"password\": \"Password\",\"role\":\"{role}\",\"dailycalorielimit\":100}}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.PostAsync("/api/users", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateUserResult(response, new User
            {
                UserName = $"{unique}",
                FirstName = "Name",
                LastName = "New",
                EmailAddress = $"{unique}@gmail.com",
                DailyCalorieLimit = 100,
                Role = role,
                Id = 11
            });
        }

        [Fact]
        public async Task PatchUser_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();
            string unique = Guid.NewGuid().ToString();
            string json = $"{{\"emailaddress\": \"{unique}@gmail.com\" }}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.PatchAsync("/api/users/own", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateUserResult(response, new User
            {
                UserName = "Aide4th",
                FirstName = "Aide",
                LastName = "New",
                EmailAddress = $"{unique}@gmail.com",
                DailyCalorieLimit = 100,
                Role = UserRole.Admin,
                Id = 2
            });
        }

        [Theory]
        [InlineData(8, UserRole.Admin)]
        [InlineData(9, UserRole.Regular)]
        [InlineData(10, UserRole.UserManager)]
        public async Task PatchUser_PatchOtherUser_EndpointReturnSuccessAndValidateContent(int id, UserRole role)
        {
            // Arrange
            var client = _factory.CreateClient();
            string unique = Guid.NewGuid().ToString();
            string json = $"{{ \"emailaddress\": \"{unique}@gmail.com\" ,"
            + $"\"username\":\"{unique}\" }}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.PatchAsync($"/api/users/{id}", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateUserResult(response, new User
            {
                UserName = $"{unique}",
                FirstName = "Name",
                LastName = "New",
                EmailAddress = $"{unique}@gmail.com",
                DailyCalorieLimit = 100,
                Role = role,
                Id = id
            });
        }

        [Theory]
        [InlineData(8, UserRole.Regular)]
        [InlineData(9, UserRole.UserManager)]
        [InlineData(10, UserRole.Admin)]
        public async Task PatchUser_PatchOtherUserRole_EndpointReturnSuccessAndValidateContent(int id, UserRole role)
        {
            // Arrange
            var client = _factory.CreateClient();
            string json = $"{{ \"role\": \"{role}\" ,"
            + $"\"dailycalorielimit\":\"{200}\" }}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.PatchAsync($"/api/users/{id}", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateUserResult(response, new User
            {
                UserName = $"user{id}",
                FirstName = "Name",
                LastName = "New",
                EmailAddress = $"email{id}@gmail.com",
                DailyCalorieLimit = 200,
                Role = role,
                Id = id
            });
        }


        [Fact]
        public async Task Post_ChangeOtherUserRole_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();
            string json = $"{{ \"oldpassword\": \"Password\" ,\"newpassword\":\"NewPassword\" }}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.PostAsync($"/api/users/own/change-password", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            string responseString = await response.Content.ReadAsStringAsync();
            ApiResponse requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse>(responseString);
            Assert.NotNull(requestResponse);
            Assert.True(requestResponse.IsSuccessful);
            Assert.Equal("Successful", requestResponse.Message);

            response.Dispose();
        }

        [Fact]
        public async Task SearchUser_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            await GetAndSetLoginAccountToken(client);
            var response = await client.GetAsync($"/api/users?filter=(DateCreated gt '2020-02-02') AND (Role eq 'Admin')&start=1&limit=10");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            string responseString = await response.Content.ReadAsStringAsync();
            ApiObjectResponse<SearchResult<User>> requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjectResponse<SearchResult<User>>>(responseString);
            Assert.NotNull(requestResponse);
            Assert.True(requestResponse.IsSuccessful);
            Assert.Equal("Successful", requestResponse.Message);

            Assert.NotNull(requestResponse.Data);
            Assert.Equal(3, requestResponse.Data.TotalCount);
            Assert.NotNull(requestResponse.Data.Results);
            Assert.True(requestResponse.Data.Results.All(b => b.Role == UserRole.Admin));

            response.Dispose();
        }
    }
}
