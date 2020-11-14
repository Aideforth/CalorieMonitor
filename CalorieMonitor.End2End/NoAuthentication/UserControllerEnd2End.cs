using CalorieMonitor.Core.Entities;
using CalorieMonitor.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.End2End.NoAuthentication
{
    public class UserControllerEnd2End : BaseControllerEnd2End
    {
        public UserControllerEnd2End(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        [Fact]
        public async Task Post_RegisterAccount_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();
            string unique = Guid.NewGuid().ToString();
            string json = $"{{\"firstName\": \"Name\",\"lastName\": \"New\",\"username\": \"{unique}\", \"emailaddress\": \"{unique}@gmail.com\"," +
                $"\"password\": \"Password\", \"DailyCalorieLimit\":100}}";
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/users", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            await ValidateUserResult(response, new User
            {
                UserName = unique,
                FirstName = "Name",
                LastName = "New",
                EmailAddress = $"{unique}@gmail.com",
                DailyCalorieLimit = 100,
                Id = 11,
                Role = Core.Enums.UserRole.Regular
            });
        }

        [Fact]
        public async Task Post_LoginAccount_EndpointReturnSuccessAndValidateContent()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var content = new StringContent("{\"username\": \"Aide4th\", \"password\": \"Password\"}", System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/users/login", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            ApiObjectResponse<TokenInfo> requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjectResponse<TokenInfo>>(await response.Content.ReadAsStringAsync());
            ValidateLogin(requestResponse);

            response.Dispose();
        }
    }
}
