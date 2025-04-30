using Microsoft.AspNetCore.Mvc;
using TicTacToeGame.Helpers.Constants;
using TicTacToeGame.Helpers.Mappers;
using TicTacToeGame.Models.Dtos.Users;
using TicTacToeGame.Models.Requests.Users;
using TicTacToeGame.Services.Interfaces;

namespace TicTacToeGame.Controllers;

[ApiController]
[Route(AppConstants.AppControllers.IdentityController.BasePath)]
public class IdentityController : ControllerBase
{
    private readonly IUnitOfWork UOW;

    public IdentityController(IUnitOfWork uow)
    {
        UOW = uow;
    }

    [HttpPost(AppConstants.AppControllers.IdentityController.Login)]
    public async Task<IActionResult> Login(UserLoginRequest request)
    {
        if (await UOW.UserService.LoginAsync(request.Username, request.Password))
        {
            return BadRequest("Invalid username or password.");
        }

        // Generate JWT token here and return it to the user
        // For simplicity, we are just returning a success message
        return Ok("Login successful. Token: [JWT_TOKEN]");
    }

    [HttpPost(AppConstants.AppControllers.IdentityController.Register)]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        if (await UOW.UserService.RegisterAsync(request.ToRegisterUserDto()))
        {
            return BadRequest("User already exists.");
        }

        // For simplicity, we are just returning a success message
        return Ok("Registration successful.");
    }

    [HttpGet(AppConstants.AppControllers.IdentityController.Logout)]
    public IActionResult Logout()
    {
        return Ok("Logout successful.");
    }
}