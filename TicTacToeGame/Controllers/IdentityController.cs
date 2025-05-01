using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Helpers.Options;
using TicTacToeGame.Models;
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
    private ISimpleUserService UserService { get; set; }
    private ITokenService<SimpleUser> TokenService { get; set; }
    private JwtConfigurationOptions JWTOptions { get; set; }

    public IdentityController(ISimpleUserService simpleUserService, IOptions<JwtConfigurationOptions> jwtOptions, ITokenService<SimpleUser> tokenService)
    {
        TokenService = tokenService;
        JWTOptions = jwtOptions.Value;
        UserService = simpleUserService;
    }

    [AllowAnonymous]
    [HttpPost(AppConstants.AppControllers.IdentityController.Login)]
    public async Task<IActionResult> Login(SimpleUserLoginRequest request)
    {
        var user = await UserService.LoginAsync(request.Username);
        if (user is null)
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = AppConstants.AppResponses.IdentityControllerResponses.LoginResponses.
                UsernameAlreadyExisted,
                Errors = AppConstants.AppResponses.IdentityControllerResponses.LoginResponses.UsernameAlreadyExistedErrors,
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
                AccessToken = TokenService.GenerateAccessToken(user),
                ExpiresIn = JWTOptions.Expiration
            }
        });
    }

    // [AllowAnonymous]
    // [HttpPost(AppConstants.AppControllers.IdentityController.Register)]
    // public async Task<IActionResult> Register(RegisterUserRequest request)
    // {
    //     if (!await UOW.UserService.RegisterAsync(request.ToRegisterUserDto()))
    //     {
    //         return BadRequest(new RegisterResponse
    //         {
    //             Success = false,
    //             Message = AppConstants.AppResponses.IdentityControllerResponses.RegisterResponses.RegistrationFailed,
    //             Errors = AppConstants.AppResponses.IdentityControllerResponses.RegisterResponses.RegistrationFailedErrors,
    //         });
    //     }

    //     // For simplicity, we are just returning a success message
    //     return Ok(new RegisterResponse
    //     {
    //         Success = true,
    //         Message = AppConstants.AppResponses.IdentityControllerResponses.RegisterResponses.RegistrationSuccessful,
    //     });
    // }

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
        var user = await UserService.GetUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound(new GetSimpleUserInfoResponse
            {
                Success = false,
                Message = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.UserNotFound,
                Errors = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.UserNotFoundErrors,
            });
        }

        return Ok(new GetSimpleUserInfoResponse
        {
            Success = true,
            Message = AppConstants.AppResponses.IdentityControllerResponses.GetUserInfoResponses.GetInfoSuccessful,
            Data = user
        });
    }
}