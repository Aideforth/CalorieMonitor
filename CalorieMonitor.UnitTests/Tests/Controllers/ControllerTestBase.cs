using CalorieMonitor.Core.Implementations;
using CalorieMonitor.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CalorieMonitor.UnitTests.Tests.Controllers
{
    public class ControllerTestBase
    {
        protected void ValidateApiError<T>(IActionResult response, int code, string message) where T : ObjectResult
        {
            Assert.IsType<T>(response);
            T result = response as T;
            var resultValue = result.Value as ApiResponse;

            Assert.NotNull(resultValue);
            Assert.False(resultValue.IsSuccessful);
            Assert.Equal(message, resultValue.Message);
            Assert.Equal(code, result.StatusCode);
        }

        protected void ValidateBadRequest(IActionResult response, string field, string message)
        {
            Assert.IsType<BadRequestObjectResult>(response);
            BadRequestObjectResult result = response as BadRequestObjectResult;

            var resultValue = result.Value as SerializableError;
            Assert.NotNull(resultValue);
            Assert.True(resultValue.ContainsKey(field));
            Assert.Equal(message, ((string[])resultValue[field])[0]);
            Assert.Equal(400, result.StatusCode);
        }
    }
}
