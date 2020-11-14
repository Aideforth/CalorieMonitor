using CalorieMonitor.Core.Exceptions;
using CalorieMonitor.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CalorieMonitor.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult PrepareException(Exception ex)
        {
            if (ex is FailureException)
            {
                return StatusCode(500, new ApiResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                });
            }

            if (ex is BusinessException || ex is InvalidFilterException || ex is FormatException)
            {
                return StatusCode(400, new ApiResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                });
            }

            if (ex is NotFoundException)
            {
                return NotFound(new ApiResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                });
            }

            if (ex is UnauthorizedException)
            {
                return Unauthorized(new ApiResponse
                {
                    IsSuccessful = false,
                    Message = ex.Message
                });
            }

            return StatusCode(500, new ApiResponse
            {
                IsSuccessful = false,
                Message = "An error occurred, please try again"
            });
        }
    }
}
