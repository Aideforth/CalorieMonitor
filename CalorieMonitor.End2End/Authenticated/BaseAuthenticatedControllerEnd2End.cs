using CalorieMonitor.Core.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace CalorieMonitor.End2End.Authenticated
{
    public class BaseAuthenticatedControllerEnd2End : BaseControllerEnd2End
    {
        public BaseAuthenticatedControllerEnd2End(WebApplicationFactory<Startup> factory) : base(factory)
        {
        }

        protected async Task GetAndSetLoginAccountToken(HttpClient client, string username="Aide4th", string password = "Password")
        {
            var content = new StringContent($"{{\"username\": \"{username}\", \"password\": \"{password}\"}}", System.Text.Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/api/users/login", content);

            response.EnsureSuccessStatusCode(); // Status Code 200-299
            ApiObjectResponse<TokenInfo> requestResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiObjectResponse<TokenInfo>>(await response.Content.ReadAsStringAsync());
            ValidateLogin(requestResponse);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {requestResponse.Data.Token}");
        }
    }
}
