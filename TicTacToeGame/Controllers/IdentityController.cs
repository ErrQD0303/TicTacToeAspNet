using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Helpers.Options;
using TicTacToeGame.Models.Dtos.Tokens;
using TicTacToeGame.Models.Requests.Users;
using TicTacToeGame.Models.Responses.Users;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Controllers;

[Authorize]
[ApiController]
[Route(AppConstants.AppControllers.IdentityController.BasePath)]
public class IdentityController : ControllerBase
{
    private readonly IUnitOfWork UOW;
    private JwtConfigurationOptions JWTOptions { get; set; }

    public IdentityController(IUnitOfWork uow, IOptions<JwtConfigurationOptions> jwtOptions)
    {
        JWTOptions = jwtOptions.Value;
        UOW = uow;
    }

    [AllowAnonymous]
    [HttpPost(AppConstants.AppControllers.IdentityController.Login)]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        var loggedInUser = await UOW.UserService.LoginAsync(request.Username, request.Password);
        if (loggedInUser is null)
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = AppConstants.AppResponses.IdentityControllerResponses.LoginResponses.
                InvalidUsernameOrPassword,
                Errors = AppConstants.AppResponses.IdentityControllerResponses.LoginResponses.InvalidUsernameOrPasswordErrors,
                Data = null
            });
        }

        // Generate JWT token here and return it to the user
        // For simplicity, we are just returning a success message
        return Ok(new LoginResponse
        {
            Message = AppConstants.AppResponses.IdentityControllerResponses.LoginResponses.LoginSuccessful,
            Data = new LoginTokenDto
            {
                TokenType = JWTOptions.TokenType,
                AccessToken = UOW.TokenService.GenerateAccessToken(loggedInUser.ToAppUser()),
                ExpiresIn = JWTOptions.Expiration
            }
        });
    }

    [AllowAnonymous]
    [HttpPost(AppConstants.AppControllers.IdentityController.Register)]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        if (!await UOW.UserService.RegisterAsync(request.ToRegisterUserDto()))
        {
            return BadRequest(new RegisterResponse
            {
                Success = false,
                Message = AppConstants.AppResponses.IdentityControllerResponses.RegisterResponses.RegistrationFailed,
                Errors = AppConstants.AppResponses.IdentityControllerResponses.RegisterResponses.RegistrationFailedErrors,
            });
        }

        // For simplicity, we are just returning a success message
        return Ok(new RegisterResponse
        {
            Success = true,
            Message = AppConstants.AppResponses.IdentityControllerResponses.RegisterResponses.RegistrationSuccessful,
        });
    }

    [HttpGet(AppConstants.AppControllers.IdentityController.Logout)]
    public IActionResult Logout()
    {
        return Ok(new LogoutResponse
        {
            Success = true,
            Message = AppConstants.AppResponses.IdentityControllerResponses.LogoutReponses.LogoutSuccessful,
        });
    }

    [HttpGet(AppConstants.AppControllers.IdentityController.GetInfo)]
    public async Task<IActionResult> GetInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new GetUserInfoResponse
            {
                Success = false,
                Message = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.UserIdNotFound,
                Errors = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.UserIdNotFoundErrors,
            });
        }
        var user = await UOW.UserService.GetUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new GetUserInfoResponse
            {
                Success = false,
                Message = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.UserNotFound,
                Errors = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.UserNotFoundErrors,
            });
        }

        return Ok(new GetUserInfoResponse
        {
            Success = true,
            Message = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.GetInfoSuccessful,
            Data = user
        });
    }
}