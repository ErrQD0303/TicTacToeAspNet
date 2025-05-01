using Microsoft.AspNetCore.Identity;
using TicTacToeGame.Models;
using TicTacToeGame.Models.Dtos.Users;

namespace TicTacToeGame.Helpers;

public class PasswordHelper
{
    public static PasswordHasher<HashPasswordUserDto> PasswordHasher { get; private set; }

    static PasswordHelper()
    {
        PasswordHasher = new PasswordHasher<HashPasswordUserDto>();
    }

    public static string HashPassword(HashPasswordUserDto user, string password)
    {
        return PasswordHasher.HashPassword(user, password);
    }

    public static bool VerifyPassword(HashPasswordUserDto user, string password, string hashedPassword)
    {
        var result = PasswordHasher.VerifyHashedPassword(user, hashedPassword, password);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}