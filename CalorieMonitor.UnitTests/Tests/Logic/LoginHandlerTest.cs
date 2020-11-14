using CalorieMonitor.Core.Entities;
using CalorieMonitor.Logic.Implementations;
using CalorieMonitor.UnitTests.Utilities;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Logic
{
    public class LoginHandlerTest
    {
        [Fact]
        public void TestLoginUserAndGenerateToken_Valid_ReturnsToken()
        {
            //Arrange
            Mock<IConfiguration> mockConfiguration;
            mockConfiguration = new Mock<IConfiguration>();
            string audienceAndIssuer = "https://localhost:5001/";
            mockConfiguration.SetupGet(v => v["Jwt:SecretKey"]).Returns("EncodedSignInKey");
            mockConfiguration.SetupGet(v => v["Jwt:Issuer"]).Returns(audienceAndIssuer);
            mockConfiguration.SetupGet(v => v["Jwt:Audience"]).Returns(audienceAndIssuer);
            User user = UserUtil.GenerateRecords(DateTime.Now, 1)[0];

            //Act
            string token = new LoginHandler(mockConfiguration.Object).LoginUserAndGenerateToken(user);

            //Assert
            Assert.NotNull(token);
            JwtSecurityToken JwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            Assert.NotNull(JwtToken);
            Assert.Equal(audienceAndIssuer, JwtToken.Issuer);
            Assert.Contains(audienceAndIssuer, JwtToken.Audiences);
            Assert.Equal(user.Id.ToString(), JwtToken.Claims.First(v => v.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(user.FirstName, JwtToken.Claims.First(v => v.Type == ClaimTypes.Name).Value);
            Assert.Equal(user.EmailAddress, JwtToken.Claims.First(v => v.Type == ClaimTypes.Email).Value);
            Assert.Equal(user.Role.ToString(), JwtToken.Claims.First(v => v.Type == ClaimTypes.Role).Value);
        }
    }
}
